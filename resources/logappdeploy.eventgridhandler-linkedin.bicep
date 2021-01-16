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

// LinkedIn
param linkedInUsername string

var metadata = {
    longName: '{0}-${name}-${env}-${locationCode}{1}'
    shortName: '{0}${name}${env}${locationCode}'
}

var linkedInConnector = {
    id: '${subscription().id}/providers/Microsoft.Web/locations/${location}/managedApis/linkedinv2'
    connectionId: '${resourceGroup().id}/providers/Microsoft.Web/connections/${format(format(metadata.longName, 'apicon', '-linkedin-{0}'), linkedInUsername)}'
    connectionName: format(format(metadata.longName, 'apicon', '-linkedin-{0}'), linkedInUsername)
    location: location
}

resource apiconLinkedIn 'Microsoft.Web/connections@2016-06-01' = {
    name: linkedInConnector.connectionName
    location: linkedInConnector.location
    kind: 'V1'
    properties: {
        displayName: linkedInConnector.connectionName
        api: {
            id: linkedInConnector.id
        }
    }
}

var logicApp = {
    name: format(format(metadata.longName, 'logapp', '-eventgrid-sub-handler-linkedin-{0}'), linkedInUsername)
    location: location
}

var youtube = {
    source: 'https://www.youtube.com/xml/feeds/videos.xml?channel_id=${youTubeChannelId}'
    type: youTubeEventType
    titleSegment: youTubeTitleSegment
}

resource logapp 'Microsoft.Logic/workflows@2019-05-01' = {
    name: logicApp.name
    location: logicApp.location
    properties: {
        state: 'Enabled'
        parameters: {
            '$connections': {
                value: {
                    linkedin: {
                        id: linkedInConnector.id
                        connectionId: linkedInConnector.connectionId
                        connectionName: apiconLinkedIn.name
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
                        Build_LinkedIn_Post: {
                            type: 'Compose'
                            runAfter: {}
                            inputs: {
                                title: '@{trim(first(outputs(\'Split_Title\')))} | @{trim(first(skip(outputs(\'Split_Title\'), 1)))}'
                                body: '@triggerBody()?[\'data\']?[\'description\']'
                                link: '@triggerBody()?[\'data\']?[\'link\']'
                            }
                        }
                    }
                    else: {
                        actions: {
                            Cancel_Posting_LinkedIn: {
                                type: 'Terminate'
                                runAfter: {}
                                inputs: {
                                    runStatus: 'Cancelled'
                                }
                            }
                        }
                    }
                }
                Post_LinkedIn: {
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
                                name: '@parameters(\'$connections\')[\'linkedin\'][\'connectionId\']'
                            }
                        }
                        path: '/v2/people/shares'
                        body: {
                            distribution: {
                                linkedInDistributionTarget: {
                                    visibleToGuest: true
                                }
                            }
                            text: {
                                text: '@{outputs(\'Build_LinkedIn_Post\')?[\'title\']}\n\n@{outputs(\'Build_LinkedIn_Post\')?[\'body\']}'
                            }
                            content: {
                                'content-url': '@outputs(\'Build_LinkedIn_Post\')?[\'link\']'
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
