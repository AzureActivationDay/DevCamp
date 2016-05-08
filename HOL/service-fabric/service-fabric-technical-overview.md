<properties
   pageTitle="Service Fabric terminology overview | Microsoft Azure"
   description="A terminology overview of Service Fabric. Discusses key terminology concepts and terms used in the rest of the documentation."
   services="service-fabric"
   documentationCenter=".net"
   authors="msfussell"
   manager="timlt"
   editor="chackdan;subramar"/>

<tags
   ms.service="service-fabric"
   ms.devlang="dotnet"
   ms.topic="article"
   ms.tgt_pltfrm="NA"
   ms.workload="NA"
   ms.date="04/05/2016"
   ms.author="msfussell"/>

# Service Fabric terminology overview

Service Fabric is a distributed systems platform that makes it easy to package, deploy, and manage scalable and reliable microservices. This topic details the terminology used by Service Fabric in order to understand the terms used elsewhere in the documentation.

## Infrastructure concepts
**Cluster**: A network-connected set of virtual or physical machines into which your microservices are deployed and managed.  Clusters can scale to thousands of machines.

**Node**: A machine or VM that is part of a cluster is called a node. Each node is assigned a node name (a string). Nodes have characteristics such as placement properties. Each machine or VM has an auto-start Windows service, `FabricHost.exe` which starts running upon boot and then this service starts two executables: `Fabric.exe` and `FabricGateway.exe`. These two executables make up the node. For testing scenarios, you can host multiple nodes on a single PC, VM by running multiple instances of `Fabric.exe` and `FabricGateway.exe`. 

## Application concepts
**Application Type**: The name/version assigned to a collection of service types. This information is defined in an `ApplicationManifest.xml` file, embedded in an application package directory which is then copied to the Service Fabric cluster's image store. You can then create a named application from this application type within the cluster. 

Read the [Application Model](service-fabric-application-model.md) article for more information.

**Application Package**: A disk directory containing the application type's `ApplicationManifest.xml` file. This file references the service packages for each service type that makes up the application type. The files in the application package directory are copied to Service Fabric cluster's image store. For example, an application package for an email application type could contain references to a queue service package, a frontend service package, and a database service package.

**Named Application**: After an application package has been copied to the image store, you can create an instance of the application within the cluster by specifying the application package's application type (using its name/version). Each application type instance is assigned a URI name that looks like this: `"fabric:/MyNamedApp"`. Within a cluster, you can create multiple named applications from a single application type. You can also create named applications from different application types. Each named application is managed and versioned independently.      

**Service Type**: The name/version assigned to a service's code packages, data packages, and configuration packages. This information is defined in a `ServiceManifest.xml` file, embedded in a service package directory and the service package directory is then referenced by an application package's `ApplicationManifest.xml` file. Within the cluster, after creating a named application, you can create a named service from one of the application type's service types. The service type's `ServiceManifest.xml` file describes the service.

Read the [Application Model](service-fabric-application-model.md) article for more information.

There are two types of services:

- **Stateless:** Use a stateless service when the service's persistent state is stored in an external storage service such as Azure Storage, Azure SQL Database, or Azure DocumentDB. You should also use a stateless service when the service has no persistent storage at all. For example, a calculator service where values are passed to the service, a computation is performed using these values, and a result is returned.

- **Stateful:** Use a stateful service when you want Service Fabric to manage your service's state via its Reliable Collections or Reliable Actors programming models. When creating a named service, you specify how many partitions you want to spread your state over (for scalability) and you specify how many times to replicate your state across nodes (for reliability). Each named service has a single primary replica and multiple secondary replicas. You modify your named service's state by writing to the primary replica; Service Fabric then replicas this state to all the secondary replicas keeping your state in sync. When a primary replica fails, Service Fabric automatically detects this, promotes an existing secondary replica to a primary replica and then creates a new secondary replica.  

**Service Package**: A disk directory containing the service type's `ServiceManifest.xml` file. This file references the code, static data, and configuration packages for the service type. The files in the service package directory are referenced by the application type's `ApplicationManifest.xml` file. For example, a service package could refer to the code, static data, and configuration packages that make up a database service.

**Named Service**: After created a named application, you can create an instance of one of its service types within the cluster by specifying the service type (using its name/version). Each service type instance is assigned a URI name scoped under its named application's URI. For example, if you create a "MyDatabase" named service within a "MyNamedApp" named application, the URI looks like this: `"fabric:/MyNamedApp/MyDatabase"`. Within a named application, you can create several named services. Each named service can have its own partition scheme and instance/replica counts.

**Code Package**: A disk directory containing the service type's executable files (typically EXE/DLL files). The files in the code package directory are referenced by the service type's `ServiceManifest.xml` file. When a named service is created, the files in the code package are copied to the node(s) that Service Fabric selects to run the named service and then the code starts running. There are two types of code package executables:

- **Guest executables**: Executables that run as-is on the host operating system (Windows or Linux). That is, these executables do not link to or reference any Service Fabric runtime files and therefore do not use any of Service Fabric programming models. These executables are unable to leverage some Service Fabric features such as the naming service for endpoint discovery and they cannot report load metrics specific to each service instance.

- **Service Host Executables**: These are executables that use Service Fabric programming models by linking to Service Fabric runtime files. This ties portions of the executable's code to Service Fabric thus enabling additional features. For example, a named service instance can register endpoints with Service Fabric's Naming Service and can also report load metrics.      

**Data Package**: A disk directory containing the service type's static, read-only data files (typically photo, sound, and video files). The files in the data package directory are referenced by the service type's `ServiceManifest.xml` file. When a named service is created, the files in the data package are copied to the node(s) that Service Fabric selects to run the named service and then the code starts running; the code can now access the data files.

**Configuration Package**: A disk directory containing the service type's static, read-only configuration files (typically text files). The files in the configuration package directory are referenced by the service type's `ServiceManifest.xml` file. When a named service is created, the files in the configuration package are copied to the node(s) that Service Fabric selects to run the named service and then the code starts running; the code can now access the configuration files.

**Partition Scheme**: When creating a named service, you specify a partition scheme. Services with large amounts of state split the data across partitions which spreads it across the cluster's nodes. This allows your named service's state to scale. Within a partition, stateless named services have instances while stateful named services have replicas. Usually, stateless named services only ever have 1 partition since they have no internal state. The partition instances provide for availability; if one instance fails, other instances continue to operate normally and then Service Fabric will create a new instance. Stateful named services maintain their state within replicas and each partition has its own replica set with all the state being kept in sync. Should a replica fail, Service Fabric builds a new replica from the existing replicas.

Read the [Partition Service Fabric reliable services](service-fabric-concepts-partitioning.md) article for more information.

## System services
There are system services that are created in every cluster that provide the platform capabilites of Service Fabric.

**Naming Service**: Each Service Fabric cluster has a Naming service which resolves service names to a location in the cluster and enables you to manage the service names and properties. This is just like an internet Domain Name Service (DNS) for the cluster. Using the Naming service, clients can securely communicate with any node in the cluster to resolve a service name and its location (i.e. get hold of the actual machine IP address and port where it is currently running). Using the communication client APIs, you can develop services and clients capable of resolving the current network location despite applications being moved within the cluster for example due to failures, resource balancing or the re-sizing of the cluster.

Read the [Communicate with services](service-fabric-connect-and-communicate-with-services.md) article for more information on using the client and service communication API that work in conjunction with the Naming service.

**Image Store Service**: Each Service Fabric cluster has an Image store service where deployed, versioned application packages are kept. You must copy the contents of an application package to the image store service and then register the application type contained within that application package. Then, after the application type has been provisioned, you can create named applications of it. You can unregister an application type from the Image store service after all its named applications have been deleted. 

Read the [Deploy an application](service-fabric-deploy-remove-applications.md) article for more information on deploying applications to the Image store service.

## Built-in programming models
There are .NET Framework programming models available for you to build Service Fabric services:

**Reliable Services**: An API to build stateless and stateful services based. Stateful service store their state in Reliable Collections (such as a dictionary or a queue). You also get to plug in a variety of communication stacks such as Web API and Windows Communication Foundation (WCF).

**Reliable Actors** - An API to build stateless and stateful objects through the virtual Actor programming model. This model can be useful when you have lots of independent units of computation/state. Because this model uses a turn-based threading model, it is best to avoid code that calls out to other actors or services since an individual actor cannot process other incoming requests until all its outbound requests have completed.

Read the [Choose a Programming Model for your service](service-fabric-choose-framework.md) article for more information.

<!--Every topic should have next steps and links to the next logical set of content to keep the customer engaged-->
## Next steps
To learn more about Service Fabric

- [Overview of Service Fabric](service-fabric-overview.md)
- [Why a microservices approach to building applications?](service-fabric-overview-microservices.md)
- [Application scenarios](service-fabric-application-scenarios.md)
