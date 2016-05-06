# PaaS

## Demo 1 - Deploying a Website to Azure

Prerequisites:

- An active Azure subscription.
- The firework application (Cache-Redis-Firework-SignalR) has been loaded in Visual Studio. Compiled and verified to work locally.
- Create a CloudService with a Web- and a WorkerRole and deploy it to a new CloudService in Azure.

Steps:

1. Open Ibiza portal and click the **NEW** button at the lower-left corner.
1. Select **Website** and provision a new website.
1. Click on Browse to show that the Website slot is live and can receive traffic. Nothing is deployed yet.
1. Run the firework application locally. Explain this is a regular ASP.Net application (your application code) that you want to deploy to Azure.
1. In **Solution Explorer**, right-click the web project and select **Publish**.
1. Follow the publish wizard to publish the application to the website we just provisioned.
1. Test the application. Invite audience to participate if possible.
1. Switch back to slide to explain what just happened.

## Demo 2 - Azure Resource Manager

This is a quick demo of Azure Resource Manager demo.

1. Go to http://portal.azure.com.
1. Create an Azure Resource Group.
1. Provision a Web App in the Resource Group.
1. Provision an Azure SQL Database in the Resource Group.
1. Open Resource Group container blade. Show the Web App and database resources are contained within the Resource Group.
1. Review **Monitoring** for Web App and database events from an application lifecycle container perspective.
1. (Optional) Review **Billing** to measure total and itemized costs for the resources contained in the Resource Group.
1. Delete the Azure Resource Group. Show the Web App and database no longer exist since the container is now gone.


## Demo 3 - Hello World

1. Create a Cloud Service with a Web Role and Worker Role1. Launch, explain local emulator.
1. Show publish menu and start the publish. Explain that this takes a few minutes and therefor you have already prepared this step earlier.
1. Show the current management portal http://manage.windowsazure.com with the CloudService deployed in "Prerequisites". Show the instances and also browse to the web application deployed.