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

// Function App
param functionName string = 'FetchAsync'

// YouTube
param youTubeChannelId string
param youTubePublishedEventType string = 'com.youtube.video.published'
param youTubeConvertedEventType string = 'com.youtube.video.converted'

var metadata = {
    longName: '{0}-${name}-${env}-${locationCode}{1}'
    shortName: '{0}${name}${env}${locationCode}'
}

var logicApp = {
    name: format(metadata.longName, 'logapp', '-eventgrid-sub-handler-relay')
    location: location
}

var functionApp = {
    name: format(metadata.longName, 'fncapp', '')
    functionResourceId: resourceId('Microsoft.Web/sites/functions', format(metadata.longName, 'fncapp', ''), functionName)
}

var eventGridTopic = {
    name: format(metadata.longName, 'evtgrd', '-topic')
    resourceId: resourceId('Microsoft.EventGrid/topics', format(metadata.longName, 'evtgrd', '-topic'))
}

var youtube = {
    source: 'https://www.youtube.com/xml/feeds/videos.xml?channel_id=${youTubeChannelId}'
    publishedType: youTubePublishedEventType
    convertedType: youTubeConvertedEventType
}

resource logapp 'Microsoft.Logic/workflows@2019-05-01' = {
    name: logicApp.name
    location: logicApp.location
    properties: {
        state: 'Enabled'
        parameters: {
            functionAppKey: {
                value: listKeys(functionApp.functionResourceId, '2020-06-01').default
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
                functionAppName: {
                    type: 'string'
                    defaultValue: functionApp.name
                }
                functionAppKey: {
                    type: 'string'
                    defaultValue: ''
                }
                publishedYouTubeSource: {
                    type: 'string'
                    defaultValue: youtube.source
                }
                publishedYouTubeType: {
                    type: 'string'
                    defaultValue: youtube.publishedType
                }
                convertedYouTubeSource: {
                    type: 'string'
                    defaultValue: youtube.source
                }
                convertedYouTubeType: {
                    type: 'string'
                    defaultValue: youtube.convertedType
                }
                eventGridTopicEndpoint: {
                    type: 'string'
                    defaultValue: ''
                }
                eventGridTopicKey: {
                    type: 'string'
                    defaultValue: ''
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
                                    '@triggerBody()?[\'source\']'
                                    '@parameters(\'publishedYouTubeSource\')'
                                ]
                            }
                            {
                                equals: [
                                    '@triggerBody()?[\'type\']'
                                    '@parameters(\'publishedYouTubeType\')'
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
                Build_CloudEvents_Payload: {
                    type: 'Compose'
                    runAfter: {
                        Proceed_Only_If_Published: [
                            'Succeeded'
                        ]
                    }
                    inputs: {
                        id: '@guid()'
                        specversion: '1.0'
                        source: '@parameters(\'convertedYouTubeSource\')'
                        type: '@parameters(\'convertedYouTubeType\')'
                        time: '@utcNow()'
                        datacontenttype: 'application/cloudevents+json'
                        data: '@body(\'Fetch_YouTube_Video_Details\')'
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
