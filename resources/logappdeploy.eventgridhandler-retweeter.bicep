// Resource name
param name string

// Provisioning environment
param env string {
    allowed: [
        'dev'
        'test'
        'prod'
    ]
    default: 'dev'
}

// Resource location
param location string = resourceGroup().location

// Resource location code
param locationCode string = 'krc'

// Twitter
param twitterProfileId string
param twitterEventType string = 'com.twitter.tweet.posted'
param retweeterProfileId string

var metadata = {
    longName: '{0}-${name}-${env}-${locationCode}{1}'
    shortName: '{0}${name}${env}${locationCode}'
}

var twitterConnector = {
    id: '${subscription().id}/providers/Microsoft.Web/locations/${location}/managedApis/twitter'
    connectionId: '${resourceGroup().id}/providers/Microsoft.Web/connections/${format(format(metadata.longName, 'apicon', '-twitter-{0}'), retweeterProfileId)}'
    connectionName: format(format(metadata.longName, 'apicon', '-twitter-{0}'), retweeterProfileId)
    location: location
}

resource apiconTwitter 'Microsoft.Web/connections@2016-06-01' = {
    name: twitterConnector.connectionName
    location: twitterConnector.location
    kind: 'V1'
    properties: {
        displayName: twitterConnector.connectionName
        api: {
            id: twitterConnector.id
        }
    }
}

var logicApp = {
    name: format(format(metadata.longName, 'logapp', '-eventgrid-sub-handler-retwitter-{0}'), retweeterProfileId)
    location: location
}

var twitter = {
    source: 'https://twitter.com/${twitterProfileId}'
    type: twitterEventType
}

resource logapp 'Microsoft.Logic/workflows@2019-05-01' = {
    name: logicApp.name
    location: logicApp.location
    properties: {
        state: 'Enabled'
        parameters: {
            '$connections': {
                value: {
                    twitter: {
                        id: twitterConnector.id
                        connectionId: twitterConnector.connectionId
                        connectionName: apiconTwitter.name
                    }
                }
            }
        }
        definition: {
            '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
            contentVersion: '1.0.0.0'
            parameters: {
                '$connections': {
                    type: 'object'
                    defaultValue: {}
                }
                acceptedTwitterSource: {
                    type: 'string'
                    defaultValue: twitter.source
                }
                acceptedTwitterType: {
                    type: 'string'
                    defaultValue: twitter.type
                }
            }
            triggers: {
                manual: {
                    type: 'Request'
                    kind: 'Http'
                    inputs: {
                        schema: {
                            type: 'object'
                            properties: {
                                id: {
                                    type: 'string'
                                }
                                specversion: {
                                    type: 'string'
                                }
                                source: {
                                    type: 'string'
                                }
                                type: {
                                    type: 'string'
                                }
                                time: {
                                    type: 'string'
                                }
                                datacontenttype: {
                                    type: 'string'
                                }
                                data: {
                                    type: 'object'
                                    properties: {
                                        TweetId: {
                                            type: 'string'
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            actions: {
                Proceed_Only_If_Accepted_Source: {
                    type: 'If'
                    runAfter: {}
                    expression: {
                        and: [
                            {
                                equals: [
                                    '@triggerBody()?[\'source\']'
                                    '@parameters(\'acceptedTwitterSource\')'
                                ]
                            }
                            {
                                equals: [
                                    '@triggerBody()?[\'type\']'
                                    '@parameters(\'acceptedTwitterType\')'
                                ]
                            }
                        ]
                    }
                    actions: {
                        Retweet_Post: {
                            type: 'ApiConnection'
                            runAfter: {}
                            inputs: {
                                method: 'POST'
                                host: {
                                    connection: {
                                        name: '@parameters(\'$connections\')[\'twitter\'][\'connectionId\']'
                                    }
                                }
                                path: '/retweet'
                                queries: {
                                    tweetId: '@triggerBody()?[\'data\']?[\'TweetId\']'
                                    trimUser: false
                                }
                            }
                        }
                    }
                    else: {
                        actions: {
                            Cancel_Process_Retweeting: {
                                type: 'Terminate'
                                runAfter: {}
                                inputs: {
                                    runStatus: 'Cancelled'
                                }
                            }
                        }
                    }
                }
            }
            outputs: {}
        }
    }
}

output logicAppName string = logapp.name
