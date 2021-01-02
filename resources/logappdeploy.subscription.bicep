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
param logicAppTimezone string = 'Korea Standard Time'
param logicAppSubscriptionTopicUri string
param logicAppSubscriptionMode string {
    allowed: [
        'subscribe'
        'unsubscribe'
    ]
    default: 'subscribe'
}

// Function App
param functionName string = 'SubscribeAsync'

var metadata = {
    longName: '{0}-${name}-${env}-${locationCode}{1}'
    shortName: '{0}${name}${env}${locationCode}'
}

var logicApp = {
    name: format(metadata.longName, 'logapp', '-subscription')
    location: location
    timezone: logicAppTimezone
    topicUri: logicAppSubscriptionTopicUri
    subscriptionMode: logicAppSubscriptionMode
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
            functionAppKey: {
                value: listKeys(functionApp.functionResourceId, '2020-06-01').default
            }
        }
        definition: {
            '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
            contentVersion: '1.0.0.0'
            parameters: {
                timezone: {
                    type: 'string'
                    defaultValue: logicApp.timezone
                }
                topicUri: {
                    type: 'string'
                    defaultValue: logicApp.topicUri
                }
                subscriptionMode: {
                    type: 'string'
                    defaultValue: logicApp.subscriptionMode
                }
                functionAppName: {
                    type: 'string'
                    defaultValue: functionApp.name
                }
                functionAppKey: {
                    type: 'string'
                    defaultValue: ''
                }
            }
            triggers: {
                Run_Daily_Scheduled_Request: {
                    type: 'Recurrence'
                    recurrence: {
                        frequency: 'Day'
                        interval: 1
                        schedule: {
                            hours: [
                                '1'
                            ]
                            minutes: [
                                0
                            ]
                        }
                        timeZone: '@parameters(\'timezone\')'
                    }
                }
            }
            actions: {
                Build_Request_Payload: {
                    type: 'Compose'
                    runAfter: {}
                    inputs: {
                        topicUri: '@parameters(\'topicUri\')'
                        mode: '@parameters(\'subscriptionMode\')'
                    }
                }
                Send_Subscription_Request: {
                    type: 'Http'
                    runAfter: {
                        Build_Request_Payload: [
                            'Succeeded'
                        ]
                    }
                    inputs: {
                        method: 'POST'
                        uri: 'https://@{parameters(\'functionAppName\')}.azurewebsites.net/api/subscribe'
                        headers: {
                            'x-functions-key': '@parameters(\'functionAppKey\')'
                        }
                        body: '@outputs(\'Build_Request_Payload\')'
                    }
                }
            }
            outputs: {}
        }
    }
}
