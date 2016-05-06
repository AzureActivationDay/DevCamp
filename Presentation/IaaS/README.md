# IaaS Demos

## Demo 1 - Provisioning a VM

1. Open Azure portal and click the **NEW** button at the upper-left corner.
2. Click on **See All** in New section. Select **Compute** in Marketplace. Navigate to Recommended resources in Compute section.
3. open the **Windows Server** category.
4. Scroll down the view and show images of different types.
5. Click on **Windows Server 2012 R2 Datacenter**, and select deplpoyment model as **Resource Manager** then click the **Create** button in the overview blade. For non-Microsoft focused audience, consider searching any Linux image in the search box instead.
6. Fill in the **Basics**, **Size**, **Settings** form and click on the **OK** button in the **Summary** section to provision the VM. Explain this will take a few minutes.
7. Open the already provisioned VM.
8. Scroll down the blade to show various of information available on the blade.
9. Click on the **Extensions** tile. 
10. On the Extensions blade, click on the **ADD** icon to bring up the extension list. Introduce that VM extensions are installable components to customize VM instances. 
11. Switch to slides to continue with VM extension introduction.

## Demo 2 - VM Extension 

1.	In Azure PowerShell, issue command: **Get-AzureVMAvailableExtension | Format-Table -Property ExtensionName, Publisher**
2.	The above cmdlet lists existing extensions. Next we’ll see how we can inject an extension to a running VM instance. In the last demo you’ve seen that you can achieve this using Azure Management Portal. Here we’ll do it using PowerShell. In this case, we’ll install Custom Script Extension to an existing Windows Server 2012 VM.
3.	Issue the following cmdlets to get a reference to the virtual machine instance:
      **$serviceName = “[cloud service that hosts the VM]”**
      **$vmName = “[name of the VM]”**
      **$vm = Get-AzureVM -ServiceName $serviceName -Name $vmName**
4.	Next, issue command **Get-AzureVMExtension -VM $vm**. This lists VM extensions that are currently installed on the VM.
5.	Use the following cmdlet to enable Custom Script Extension, and instruct it to download and execute the helloworld.ps1 (this takes about 20-30 seconds):
**Set-AzureVMCustomScriptExtension -ContainerName scripts -StorageAccountName '[your storage account name]' -VM $vm -FileName ‘helloworld.ps1' -Run ‘helloworld.ps1' | Update-AzureVM -Verbose**
6. Next, we’ll retrieve and display the script execution result:
**$status = Get-AzureVM -ServiceName $serviceName -Name $vmName**
**$result = $status.ResourceExtensionStatusList.ExtensionSettingStatus.SubStatusList | Select Name, @{"Label"="Message";Expression = {$_.FormattedMessage.Message }}** 
**$result |fl**
(see screenshots in hidden slides for references)

([see this blog post](http://azure.microsoft.com/blog/2014/07/15/automating-sql-server-vm-configuration-using-custom-script-extension/) for more details on Custom Script Extension)

### Prerequisites

- Azure PowerShell v0.8 or higher has been installed and configured.
-	Desired Azure subscription has already been selected. 
-	A storage account has been provisioned under the same Azure subscription.
-	A **scripts** container has been created under the storage account with public read access.
-	A **helloworld.ps1** PowerShell script has been uploaded to the container. The content of the script is a single line: **write-output “Hello World!”**
-	PowerShell environment has been set with large font for easy reading.
-	A Windows Server 2012 VM has been provisioned.

## Demo 3 - Creating Virtual Machines with IIS and SQL VM using ARM Templates

In this demo you will learn how to create a virtual machine using ARM Templates. 

###Resources and Architecture Diagram:

The following resources are created by this template:

* 1 or 2 Windows 2012R2 IIS Web Servers.
* 1 SQL Server 2014 running on premium or standard storage.
* 1 virtual network with 2 subnets with NSG rules.
* 1 storage account for the VHD files.
* 1 Availability Set for IIS servers.
* 1 Load balancer with NATing rules.

	![Virtual Machines view in portal](images/create-architecture-diagram-vm.png?raw=true)

	_Architecture diagram for ARM Template_

The below **Deploy to Azure** button embeds an Azure ARM template which creates one or two Windows Server 2012R2 VM(s) with IIS configured using DSC. It also installs one SQL Server 2014 standard edition VM, a VNET with two subnets, NSG, loader balancer, NATing and probing rules.

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fazure%2Fazure-quickstart-templates%2Fmaster%2Fiis-2vm-sql-1vm%2Fazuredeploy.json" target="_blank"><img src="http://azuredeploy.net/deploybutton.png" /></a>

In order to create virtual machine using ARM, perform the following steps:

1. Click on the **Deploy to Azure** button which will navigate you to Azure Portal with Custom Deployment.

	![Custom deployment and its Parameters](images/create-vm-arm-parameters.png?raw=true)

	_Custom Deployment using Azure ARM - using Deploy to Azure button_

1. On the Custom Deployment blade that opens, enter the parameters:

	* **ENVPREFIXNAME**: virtual machine name (e.g. azureVM)
	* **Location**: location for the virtual machine (e.g. West US)
	* **User Name**: user name for the administrator account (e.g. adminUser)
	* **Password**: unique password for the administrator account
	* **WEBSRVVMSIZE**: unique password for the administrator account (e.g. Standard_DS1)
	* **NUMBEROFWEBSRVS**: unique password for the administrator account (e.g. 1)
	* **SQLVMSIZE**: unique password for the administrator account (e.g. Standard_DS1)
	* **STORAGEACCOUNTTYPE**: unique password for the administrator account (e.g. Standard_LRS)

1. Review the default settings, such as the **Subscription**(if you have multiple), **Resource Group**(create or select an existing group), and **Resource Group Location** and finally **Legal Temrs**, then Click **Create** which will add the resource group on to dashboard.

	![Creating a VM using ARM Dashboard](images/create-vm-arm-dashboard.png?raw=true)

	_Creating the architecture using ARM Template_

1. In the meanwhile, you can click on **Deploying Template deployment** which was created on the dashboard.

	![Status while deploying custom deployment template](images/deploying-template-deployment-status.png?raw=true)

	_Azure ARM Template deployment Status_
	
1. Once the Template Deployment succeeds, you will have WebServer with IIS Installed and SQL Server 2014 Standard deployed on a Virtual Netowrk with 2 subnets with NSG rules and a Load Balancer with NATing rules. Click on the Resource Group Tile pinned on the dashboard and then click on each resource for more details.

	![Resource Group with all resources](images/succeeded-arm-template.png?raw=true)

	_Azure ARM Template deployment Succeeds_

1. Now, the 2-tier architecture is created with all necessary resources, to check whether all the rules are applied we will deploy an ASP.NET application and a Sample Database(AdventureWorks2012).

	* Click on **dbNsg** which is a Netowork security group and delete the outbound Security rules (Prioirity - 200) - We do this step to download the below AdventureWorks2012 database.

	* Download RDPs for both Database server and Application server and login with the credentails and download the ASP.NET application content into appserver and AdventureWorks2012 database into Database server.

	* A sample ASP.NET Application content can be downloaded here : [application content](http://opsgilitytraining.blob.core.windows.net/armhackathon/cloudshop.zip)

	* Sample Database can be downloaded here : [AdventureWorks2012] (http://opsgilitytraining.blob.core.windows.net/public/AdventureWorks2012.bak)
	
	* Once you download application content onto app server extract the .zip file and copy the content and past in C:\inetpub\wwwroot.
	
		![Copied content into inetpub](images/inetpub.png?raw=true)

		_Copy content in wwwroot_

	* Open Database Server, and Open SQL Server Management Studio 2014 login with Windows Authentication for restoring the AdventureWorks2012 database.
	
	* Copy the .bak file to the Backup location "C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\Backup" and click **OK**.
	
		![Restoring Backup database file](images/backup-database.png?raw=true)

		_Restoring Backup database file_
		
	* In object explorer go to Security section and Login subsection Right Click and New Login and create a user with SQL Server authentication and in the default database select as **AdventureWorks2012**
	
		![Create a Login user](images/login-user.png?raw=true)

		_Create a login user_
	
	* On Left side you have **Server Roles** -> Select **public** and **sysadmin** and check in **User Mapping** whether **public** is selected or not and click **OK**
	
	* So till now we have an application content in appserver and database in Database Server. Fianlly we have to setup the NSG Outbound rule which we have deleted earlier to Database Server through portal.
		
	* In Azure Portal Click on the resource group which we just created and in the resources click on **dbNsg** and click on **All Settings** and Outbound security rules and click on add and fill the details as below:
		
		![Adding a Outbound NSG rule](images/addnsgrule-outbound.png?raw=true)

		_Adding a Outbound NSG rule_
	
	* And Finally login into AppServer and open inetpub\wwwroot and Open **Web.Config** in notepad and replace the **ConnectionString** with the below code:
	
	`<add name="DefaultConnection"
    connectionString="Data Source=tcp:{Destination-Internal-IP},1433;Initial Catalog=AdventureWorks2012;User ID={User-created-DbServer};Password={password};Encrypt=true;Trusted_Connection=false;TrustServerCertificate=true" providerName="System.Data.SqlClient"/>    
<add name="AdventureWorksEntities" connectionString="metadata=res://*/Models.AdventureWorks.csdl|res://*/Models.AdventureWorks.ssdl|res://*/Models.AdventureWorks.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=tcp:{Destination-Internal-IP},1433;initial catalog=AdventureWorks2012;Uid={User-created-DbServer};Password={password};multipleactiveresultsets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient" />`

	>NOTE: Before replacing the connection string, do change the following text with respective values
		
		1) Destination IP address : {Destination-Internal-IP} -> Destination IP
		2) User ID: {User-created-DbServer} -> User ID for SQL Authentication
		3) Password: {password} -> Password for SQL Authentication

	* Now you can verify by copying the Loadbalancer IP address onto browser and you will see an asp.net application with data populating from the DB Server.
	
		![Adding a Outbound NSG rule](images/output-demo.png?raw=true)

		_Output of the application_
	
<a name="cleanup"></a>
##Appendix - Cleanup

In this demo you will learn how to delete the virtual machines created in the previous sections, along with the related data disks created. 

### Remove VM using Azure Portal

1. Scroll to the bottom on the left pane and Click **Browse >**. Then either search in the search box at the top or scroll down and find **Virtual machines (Classic)**.

	![Clicking Browse in the left pane and search in the box](images/clicking-browse-virtualmachine.png?raw=true)

	_Clicking Browse in the left Menu_

1. A page listing all Virtual Machines will be displayed. 

	![Virtual Machines view in portal](images/virtual-machines-view-in-portal.png?raw=true)

	_Viewing all virtual machines created_

1. Click the **...** menu to find the virtual machine to delete and in the context menu that opens, click **Delete**.

	![Deleting a virtual machine](images/deleting-a-virtual-machine.png?raw=true)

	_Deleting a virtual machine_

1. In the **Confirmation** blade that opens, type the virtual machine name, select all other items to delete like disks and domain names, and click the **Delete** button.

	![Confirming the deletion of virtual machine](images/confirming-the-deletion-of-virtual-machine.png?raw=true)

	_Confirming the deletion of the virtual machine_

The virtual machine as well as other items selected in the previous step will be deleted. You can monitor the progress of this operation from the **Notifications** Hub.

Once complete, the **Virtual Machines** list will refresh and the virtual machine recently deleted will no longer appear. Follow the same instructions to delete all other virtual machines created in this lab.

##Summary

By completing this lab you have learned how to create virtual machines using several different methods: the Azure Portal interface, the Cross-Platform Command Line Tools, PowerShell and Automation Runbook. Additionally, you have seen how to attach an empty datadisk to the virtual machine, how to generate a Remote Desktop Protocol file to connect to the machine, and how to install extensions.
