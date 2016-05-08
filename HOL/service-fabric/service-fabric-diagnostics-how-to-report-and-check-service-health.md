<properties
   pageTitle="Report and check health with Azure Service Fabric | Microsoft Azure"
   description="Learn how to send health reports from your service code and how to check the health of your service by using the health monitoring tools that Azure Service Fabric provides."
   services="service-fabric"
   documentationCenter=".net"
   authors="punewa"
   manager="timlt"
   editor=""/>

<tags
   ms.service="service-fabric"
   ms.devlang="dotnet"
   ms.topic="article"
   ms.tgt_pltfrm="NA"
   ms.workload="NA"
   ms.date="03/18/2016"
   ms.author="toddabel"/>

# Report and check service health
When your services encounter problems, your ability to respond to and fix incidents and outages depends on your ability to detect the issues quickly. If you report problems and failures to the Azure Service Fabric health manager from your service code, you can use standard health monitoring tools that Service Fabric provides to check the health status.

There are two ways that you can report health from the service:

- Use [Partition](https://msdn.microsoft.com/library/system.fabric.istatefulservicepartition.aspx) or [CodePackageActivationContext](https://msdn.microsoft.com/library/system.fabric.codepackageactivationcontext.aspx) objects.  
You can use the `Partition` and `CodePackageActivationContext` objects to report the health of elements that are part of the current context. For example, code that runs as part of a replica can report health only on that replica, the partition that it belongs to, and the application that it is a part of.

- Use `FabricClient`.   
You can use `FabricClient` to report health from the service code if the cluster is not [secure](service-fabric-cluster-security.md) or if the service is running with admin privileges. This won't be true in most real-world scenarios. With `FabricClient`, you can report health on any entity that is a part of the cluster. Ideally, however, service code should only send reports that are related to its own health.

This article walks you through an example that reports health from the service code. The example also shows how the tools that Service Fabric provides can be used to check the health status. This article is intended to be a quick introduction to the health monitoring capabilities of Service Fabric. For more detailed information, you can read the series of in-depth articles about health that start with the link at the end of this article.

## Prerequisites
You must have the following installed:

   * Visual Studio 2015
   * Service Fabric SDK

## To create a local secure dev cluster
- Open PowerShell with admin privileges, and run the following commands.

![Commands that show how to create a secure dev cluster](./media/service-fabric-diagnostics-how-to-report-and-check-service-health/create-secure-dev-cluster.png)

## To deploy an application and check its health

1. Open Visual Studio as an administrator.

2. Create a project by using the **Stateful Service** template.

    ![Create a Service Fabric application with Stateful Service](./media/service-fabric-diagnostics-how-to-report-and-check-service-health/create-stateful-service-application-dialog.png)

3. Press **F5** to run the application in debug mode. The application will be deployed to the local cluster.

4. After the application is running, right-click the Local Cluster Manager icon in the notification area and select **Manage Local Cluster** from the shortcut menu to open Service Fabric Explorer.

    ![Open Service Fabric Explorer from notification area](./media/service-fabric-diagnostics-how-to-report-and-check-service-health/LaunchSFX.png)

5. The application health should be displayed as in this image. At this time, the application should be healthy with no errors.

    ![Healthy application in Service Fabric Explorer](./media/service-fabric-diagnostics-how-to-report-and-check-service-health/sfx-healthy-app.png)

6. You can also check the health by using PowerShell. You can use ```Get-ServiceFabricApplicationHealth``` to check an application's health, and you can use ```Get-ServiceFabricServiceHealth``` to check a service's health. The health report for the same application in PowerShell is in this image.

    ![Healthy application in PowerShell](./media/service-fabric-diagnostics-how-to-report-and-check-service-health/ps-healthy-app-report.png)

## To add custom health events to your service code
The Service Fabric project templates in Visual Studio contain sample code. The following steps show how you can report custom health events from your service code. Such reports will automatically show up in the standard tools for health monitoring that Service Fabric provides, such as Service Fabric Explorer, Azure portal health view, and PowerShell.

1. Reopen the application that you created previously in Visual Studio, or create a new application by using the **Stateful Service** Visual Studio template.

2. Open the Stateful1.cs file, and find the `myDictionary.TryGetValueAsync` call in the `RunAsync` method. You can see that this method returns a `result` that holds the current value of the counter because the key logic in this application is to keep a count running. If this were a real application, and if the lack of result represented a failure, you would want to flag that event.

3. To report a health event when the lack of result represents a failure, add the following steps.

    a. Add the `System.Fabric.Health` namespace to the Stateful1.cs file.

    ```csharp
    using System.Fabric.Health;
    ```

    b. Add the following code after the `myDictionary.TryGetValueAsync` call.

    ```csharp
    if (!result.HasValue)
    {
        HealthInformation healthInformation = new HealthInformation("ServiceCode", "StateDictionary", HealthState.Error);
        this.Partition.ReportReplicaHealth(healthInformation);
    }
    ```
    We report replica health because it's being reported from a stateful service. The `HealthInformation` parameter stores information about the health issue that's being reported.

    For a stateless service, use the following code.

    ```csharp
    if (!result.HasValue)
    {
        HealthInformation healthInformation = new HealthInformation("ServiceCode", "StateDictionary", HealthState.Error);
        this.Partition.ReportInstanceHealth(healthInformation);
    }
    ```

4. If your service is running with admin privileges or if the cluster is not [secure](service-fabric-cluster-security.md), you can also use `FabricClient` to report health as shown in the following steps.  

    a. Create the `FabricClient` instance after the `var myDictionary` declaration.

    ```csharp
    var fabricClient = new FabricClient(new FabricClientSettings() { HealthReportSendInterval = TimeSpan.FromSeconds(0) });
    ```

    b. Add the following code after the `myDictionary.TryGetValueAsync` call.

    ```csharp
    if (!result.HasValue)
    {
       var replicaHealthReport = new StatefulServiceReplicaHealthReport(
            this.ServiceInitializationParameters.PartitionId,
            this.ServiceInitializationParameters.ReplicaId,
            new HealthInformation("ServiceCode", "StateDictionary", HealthState.Error));
        fabricClient.HealthManager.ReportHealth(replicaHealthReport);
    }
    ```

5. Let's simulate this failure and see it show up in the health monitoring tools. To simulate the failure, comment out the first line in the health reporting code that you added earlier. After you comment out the first line, the code will look like the following example.

    ```csharp
    //if(!result.HasValue)
    {
        HealthInformation healthInformation = new HealthInformation("ServiceCode", "StateDictionary", HealthState.Error);
        this.Partition.ReportReplicaHealth(healthInformation);
    }
    ```
 This code will now fire this health report each time `RunAsync` executes. After you make the change, press **F5** to run the application.

6. After the application is running, open Service Fabric Explorer to check the health of the application. This time, Service Fabric Explorer will show that the application is unhealthy. This is because of the error that was reported from the code that we added previously.

    ![Unhealthy application in Service Fabric Explorer](./media/service-fabric-diagnostics-how-to-report-and-check-service-health/sfx-unhealthy-app.png)

7. If you select the primary replica in the tree view of Service Fabric Explorer, you will see that **Health State** indicates an error, too. Service Fabric Explorer also displays the health report details that were added to the `HealthInformation` parameter in the code. You can see the same health reports in PowerShell and the Azure portal.

    ![Replica health in Service Fabric Explorer](./media/service-fabric-diagnostics-how-to-report-and-check-service-health/replica-health-error-report-sfx.png)

This report will remain in the health manager until it is replaced by another report or until this replica is deleted. Because we did not set `TimeToLive` for this health report in the `HealthInformation` object, the report will never expire.

We recommend that health should be reported on the most granular level, which in this case is the replica. You can also report health on `Partition`.

```csharp
HealthInformation healthInformation = new HealthInformation("ServiceCode", "StateDictionary", HealthState.Error);
this.Partition.ReportPartitionHealth(healthInformation);
```

To report health on `Application`, `DeployedApplication`, and `DeployedServicePackage`, use  `CodePackageActivationContext`.

```csharp
HealthInformation healthInformation = new HealthInformation("ServiceCode", "StateDictionary", HealthState.Error);
var activationContext = FabricRuntime.GetActivationContext();
activationContext.ReportApplicationHealth(healthInformation);
```

## Next steps
[Deep dive on Service Fabric health](service-fabric-health-introduction.md)
