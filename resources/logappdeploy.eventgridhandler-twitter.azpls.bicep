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

// Logic Apps
param logicAppAcceptedEventType string = 'com.youtube.video.published'
param logicAppAcceptedTitleSegment string = '애저듣보잡'

// Function App
param functionName string = 'FetchAsync'

var metadata = {
    longName: '{0}-${name}-${env}-${locationCode}{1}'
    shortName: '{0}${name}${env}${locationCode}'
}

var twitterConnector = {
    id: '${subscription().id}/providers/Microsoft.Web/locations/${location}/managedApis/twitter'
    connectionId: '${resourceGroup().id}/providers/Microsoft.Web/connections/${format(metadata.longName, 'apicon', '-twitter-azpls')}'
    connectionName: format(metadata.longName, 'apicon', '-twitter-azpls')
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
    name: format(metadata.longName, 'logapp', '-eventgrid-sub-handler-twitter')
    location: location
    acceptedEventType: logicAppAcceptedEventType
    acceptedTitleSegment: logicAppAcceptedTitleSegment
}

var functionApp = {
    name: format(metadata.longName, 'fncapp', '')
    functionResourceId: resourceId('Microsoft.Web/sites/functions', format(metadata.longName, 'fncapp', ''), functionName)
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
            functionAppKey: {
                value: listKeys(functionApp.functionResourceId, '2020-06-01').default
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
                acceptedEventType: {
                    type: 'string'
                    defaultValue: logicApp.acceptedEventType
                }
                functionAppName: {
                    type: 'string'
                    defaultValue: functionApp.name
                }
                functionAppKey: {
                    type: 'string'
                    defaultValue: ''
                }
                acceptedTitleSegment: {
                    type: 'string'
                    defaultValue: logicApp.acceptedTitleSegment
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
                                    type: 'string'
                                }
                                traceparent: {
                                    type: 'string'
                                }
                            }
                        }
                    }
                }
            }
            actions: {
                Proceed_Only_If_Published: {
                    type: 'If'
                    runAfter: {}
                    expression: {
                        and: [
                            {
                                equals: [
                                    '@triggerBody()?[\'type\']'
                                    '@parameters(\'acceptedEventType\')'
                                ]
                            }
                        ]
                    }
                    actions: {
                        Fetch_YouTube_Video_Details: {
                            type: 'Http'
                            runAfter: {}
                            inputs: {
                                method: 'POST'
                                uri: 'https://@{parameters(\'functionAppName\')}.azurewebsites.net/api/fetch'
                                headers: {
                                    'x-functions-key': '@parameters(\'functionAppKey\')'
                                }
                                body: '@triggerBody()'
                            }
                        }
                    }
                    else: {
                        actions: {
                            Cancel_Processing_Event: {
                                type: 'Terminate'
                                runAfter: {}
                                inputs: {
                                    runStatus: 'Cancelled'
                                }
                            }
                        }
                    }
                }
                Split_Title: {
                    type: 'Compose'
                    runAfter: {
                        'Proceed_Only_If_Published': [
                            'Succeeded'
                        ]
                    }
                    inputs: '@split(body(\'Fetch_YouTube_Video_Details\')?[\'title\'], \'|\')'
                }
                Split_Description: {
                    type: 'Compose'
                    runAfter: {
                        'Proceed_Only_If_Published': [
                            'Succeeded'
                        ]
                    }
                    inputs: '@split(body(\'Fetch_YouTube_Video_Details\')?[\'description\'], \'---\')'
                }
                Process_Only_If_Title_Met: {
                    type: 'If'
                    runAfter: {
                        Split_Title: [
                            'Succeeded'
                        ]
                        Split_Description: [
                            'Succeeded'
                        ]
                    }
                    expression: {
                        and: [
                            {
                                equals: [
                                    '@trim(last(outputs(\'Split_Title\')))'
                                    '@parameters(\'acceptedTitleSegment\')'
                                ]
                            }
                        ]
                    }
                    actions: {
                        Build_Tweet_Post: {
                            type: 'Compose'
                            runAfter: {}
                            inputs: '@{trim(first(outputs(\'Split_Description\')))}\n\n@{body(\'Fetch_YouTube_Video_Details\')?[\'link\']}'
                        }
                    }
                    else: {
                        actions: {
                            Cancel_Tweeting_Post: {
                                type: 'Terminate'
                                runAfter: {}
                                inputs: {
                                    runStatus: 'Cancelled'
                                }
                            }
                        }
                    }
                }
                Post_Tweet: {
                    type: 'ApiConnection'
                    runAfter: {
                        Process_Only_If_Title_Met: [
                            'Succeeded'
                        ]
                    }
                    inputs: {
                        method: 'POST'
                        host: {
                            connection: {
                                name: '@parameters(\'$connections\')[\'twitter\'][\'connectionId\']'
                            }
                        }
                        path: '/posttweet'
                        queries: {
                            tweetText: '@{outputs(\'Build_Tweet_Post\')}'
                        }
                    }
                }
            }
            outputs: {}
        }
    }
}

output logicAppName string = logapp.name
