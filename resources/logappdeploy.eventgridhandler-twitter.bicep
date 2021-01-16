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

// YouTube
param youTubeChannelId string
param youTubeEventType string = 'com.youtube.video.converted'
param youTubeTitleSegment string

// Twitter
param twitterProfileId string
param twitterEventType string = 'com.twitter.tweet.posted'

var metadata = {
    longName: '{0}-${name}-${env}-${locationCode}{1}'
    shortName: '{0}${name}${env}${locationCode}'
}

var twitterConnector = {
    id: '${subscription().id}/providers/Microsoft.Web/locations/${location}/managedApis/twitter'
    connectionId: '${resourceGroup().id}/providers/Microsoft.Web/connections/${format(format(metadata.longName, 'apicon', '-twitter-{0}'), twitterProfileId)}'
    connectionName: format(format(metadata.longName, 'apicon', '-twitter-{0}'), twitterProfileId)
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
    name: format(format(metadata.longName, 'logapp', '-eventgrid-sub-handler-twitter-{0}'), twitterProfileId)
    location: location
}

var eventGridTopic = {
    name: format(metadata.longName, 'evtgrd', '-topic')
    resourceId: resourceId('Microsoft.EventGrid/topics', format(metadata.longName, 'evtgrd', '-topic'))
}

var youtube = {
    source: 'https://www.youtube.com/xml/feeds/videos.xml?channel_id=${youTubeChannelId}'
    type: youTubeEventType
    titleSegment: youTubeTitleSegment
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
            eventGridTopicEndpoint: {
                value: reference(eventGridTopic.resourceId, '2020-06-01', 'Full').properties.endpoint
            }
            eventGridTopicKey: {
                value: listKeys(eventGridTopic.resourceId, '2020-06-01').key1
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
                acceptedYouTubeSource: {
                    type: 'string'
                    defaultValue: youtube.source
                }
                acceptedYouTubeType: {
                    type: 'string'
                    defaultValue: youtube.type
                }
                acceptedYouTubeTitleSegment: {
                    type: 'string'
                    defaultValue: youtube.titleSegment
                }
                eventGridTopicEndpoint: {
                    type: 'string'
                    defaultValue: ''
                }
                eventGridTopicKey: {
                    type: 'string'
                    defaultValue: ''
                }
                twitterSource: {
                    type: 'string'
                    defaultValue: twitter.source
                }
                twitterType: {
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
                                        channelId: {
                                            type: 'string'
                                        }
                                        videoId: {
                                            type: 'string'
                                        }
                                        title: {
                                            type: 'string'
                                        }
                                        description: {
                                            type: 'string'
                                        }
                                        link: {
                                            type: 'string'
                                        }
                                        thumbnailLink: {
                                            type: 'string'
                                        }
                                        datePublished: {
                                            type: 'string'
                                        }
                                        dateUpdated: {
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
                                    '@parameters(\'acceptedYouTubeSource\')'
                                ]
                            }
                            {
                                equals: [
                                    '@triggerBody()?[\'type\']'
                                    '@parameters(\'acceptedYouTubeType\')'
                                ]
                            }
                        ]
                    }
                    actions: {}
                    else: {
                        actions: {
                            Cancel_Processing_Tweet: {
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
                        Proceed_Only_If_Accepted_Source: [
                            'Succeeded'
                        ]
                    }
                    inputs: '@split(triggerBody()?[\'data\']?[\'title\'], \'|\')'
                }
                Split_Description: {
                    type: 'Compose'
                    runAfter: {
                        Proceed_Only_If_Accepted_Source: [
                            'Succeeded'
                        ]
                    }
                    inputs: '@split(triggerBody()?[\'data\']?[\'description\'], \'---\')'
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
                                    '@parameters(\'acceptedYouTubeTitleSegment\')'
                                ]
                            }
                        ]
                    }
                    actions: {
                        Build_Tweet_Post: {
                            type: 'Compose'
                            runAfter: {}
                            inputs: '@{trim(first(outputs(\'Split_Title\')))}\n\n@{trim(first(outputs(\'Split_Description\')))}\n\n@{triggerBody()?[\'data\']?[\'link\']}'
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
                Build_CloudEvents_Payload: {
                    type: 'Compose'
                    runAfter: {
                        Post_Tweet: [
                            'Succeeded'
                        ]
                    }
                    inputs: {
                        id: '@guid()'
                        specversion: '1.0'
                        source: '@parameters(\'twitterSource\')'
                        type: '@parameters(\'twitterType\')'
                        time: '@utcNow()'
                        datacontenttype: 'application/cloudevents+json'
                        data: '@body(\'Post_Tweet\')'
                    }
                }
                Send_EventGrid_Tweet: {
                    type: 'Http'
                    runAfter: {
                        Build_CloudEvents_Payload: [
                            'Succeeded'
                        ]
                    }
                    inputs: {
                        method: 'POST'
                        uri: '@parameters(\'eventGridTopicEndpoint\')'
                        headers: {
                            'aeg-sas-key': '@parameters(\'eventGridTopicKey\')'
                            'Content-Type': '@outputs(\'Build_CloudEvents_Payload\')?[\'datacontenttype\']'
                            'ce-id': '@outputs(\'Build_CloudEvents_Payload\')?[\'id\']'
                            'ce-specversion': '@outputs(\'Build_CloudEvents_Payload\')?[\'specversion\']'
                            'ce-source': '@outputs(\'Build_CloudEvents_Payload\')?[\'source\']'
                            'ce-type': '@outputs(\'Build_CloudEvents_Payload\')?[\'type\']'
                            'ce-time': '@outputs(\'Build_CloudEvents_Payload\')?[\'time\']'
                        }
                        body: '@outputs(\'Build_CloudEvents_Payload\')'
                    }
                }
            }
            outputs: {}
        }
    }
}

output logicAppName string = logapp.name
