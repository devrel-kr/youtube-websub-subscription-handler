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

// Facebook
param facebookPageId string

// IFTTT
param iftttEventName string = 'facebook'
param iftttWebhookKey string {
    secure: true
}

var metadata = {
    longName: '{0}-${name}-${env}-${locationCode}{1}'
    shortName: '{0}${name}${env}${locationCode}'
}

var logicApp = {
    name: format(format(metadata.longName, 'logapp', '-eventgrid-sub-handler-facebook-{0}'), facebookPageId)
    location: location
}

var youtube = {
    source: 'https://www.youtube.com/xml/feeds/videos.xml?channel_id=${youTubeChannelId}'
    type: youTubeEventType
    titleSegment: youTubeTitleSegment
}

var facebook = {
    source: 'https://facebook.com/${facebookPageId}'
}

var ifttt = {
    endpoint: 'https://maker.ifttt.com/trigger/${iftttEventName}/with/key/${iftttWebhookKey}'
}

resource logapp 'Microsoft.Logic/workflows@2019-05-01' = {
    name: logicApp.name
    location: logicApp.location
    properties: {
        state: 'Enabled'
        parameters: {
            iftttEndpoint: {
                value: ifttt.endpoint
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
                iftttEndpoint: {
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
                                    '@triggerBody()?[\'data\']?[\'source\']'
                                    '@parameters(\'acceptedYouTubeSource\')'
                                ]
                            }
                            {
                                equals: [
                                    '@triggerBody()?[\'data\']?[\'type\']'
                                    '@parameters(\'acceptedYouTubeType\')'
                                ]
                            }
                        ]
                    }
                    actions: {}
                    else: {
                        actions: {
                            Cancel_Processing_Post: {
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
                Process_Only_If_Title_Met: {
                    type: 'If'
                    runAfter: {
                        Split_Title: [
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
                        Build_IFTTT_Post: {
                            type: 'Compose'
                            runAfter: {}
                            inputs: {
                                value1: '@{trim(first(outputs(\'Split_Title\')))} | @{trim(first(skip(outputs(\'Split_Title\'), 1)))}'
                                value2: '@replace(triggerBody()?[\'data\']?[\'description\'], \'\n\', \'<br>&nbsp;<br>\')'
                                value3: '@triggerBody()?[\'data\']?[\'link\']'
                            }
                        }
                    }
                    else: {
                        actions: {
                            Cancel_Posting_Facebook: {
                                type: 'Terminate'
                                runAfter: {}
                                inputs: {
                                    runStatus: 'Cancelled'
                                }
                            }
                        }
                    }
                }
                Send_IFTTT_Post: {
                    type: 'Http'
                    runAfter: {
                        Process_Only_If_Title_Met: [
                            'Succeeded'
                        ]
                    }
                    inputs: {
                        method: 'POST'
                        uri: '@parameters(\'iftttEndpoint\')'
                        headers: {
                            'Content-Type': 'application/json'
                        }
                        body: '@outputs(\'Build_IFTTT_Post\')'
                    }
                }
            }
            outputs: {}
        }
    }
}

output logicAppName string = logapp.name
