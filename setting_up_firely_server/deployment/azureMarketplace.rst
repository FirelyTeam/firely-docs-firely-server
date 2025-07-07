.. _deploy_azure_marketplace:

Using Firely Server from Azure Marketplace
==========================================

Firely Server is available in the Azure Marketplace, which allows you to deploy it easily on Azure Kubernetes Service (AKS) with a single click. 
This deployment method uses an embedded Helm chart, which simplifies the setup process and provides a consistent configuration experience.
The Azure Marketplace deployment is ideal for users who want to quickly get started with Firely Server on Azure without needing to manually 
configure Kubernetes resources.

In order to deploy Firely Server from the Azure Marketplace, follow these steps:

1. Go to the `Azure Marketplace <https://azuremarketplace.microsoft.com/en-us/marketplace/apps?search=Firely&page=1>`_ and search for "Firely Server".
2. Click on the "Get it now" button to start the deployment process.
3. On the first panel, provide the Azure subscription, the resource group and the cluster where Firely Server should be deployed.
4. On the second pane, provide the parameters for configuring the deployment. 
5. Click "Review + Create" to review your settings and then click "Create" to deploy Firely Server.

You can monitor the deployment progress in the Azure portal under the "Deployments" section of the selected resource group.

For the most basic deployment, you can simply specify the extension resource name and the license, and leave the other parameters at their default values.
This will deploy Firely Server in the selected kubernetes cluster as a kubernetes deployment and create a load balancer with a public IP, exposing Firely Server on port 80. 

For more advanced scenarios, you can customize the deployment by providing additional parameters as described below.

Parameters
-------------

The following parameters are available for configuring the Firely Server deployment:

- **Extension Resource Name** (``extensionResourceName``):  
  The name for the extension resource. Only alphanumeric characters are allowed, and the value must be between 6 and 30 characters long. *(Required)*

- **Enable Ingress?** (``UseIngress``):  
  Determines whether to enable Kubernetes Ingress for Firely Server. Select "Yes" to expose the service via Ingress, or "No" to disable Ingress. Note that in order to use an Ingress, you need to have an Ingress Controller already deployed in your kubernetes cluster. *(Required)*

- **Ingress Class Name** (``IngressClassName``):  
  The name of the Ingress class to use. Leave empty if not using Ingress. Only alphanumeric characters, dots, and dashes are allowed, with a maximum length of 120 characters. This should match the deployed Ingress Controller in your cluster. *(Required if Ingress is enabled)*

- **Hostname** (``Hostname``):  
  The hostname to use for Firely Server when Ingress is enabled. Must be at least 4 characters long and can include alphanumeric characters, dots, and dashes. *(Required if Ingress is enabled)*

- **Ingress TLS Secret Name** (``IngressTlsSecretName``):  
  The name of the Kubernetes secret containing the TLS certificate for Ingress. Required only if using Ingress with TLS. Can include alphanumeric characters, dots, and dashes, up to 120 characters. In order to use this option, you need to have a certificate manager (typically `cert-manager <https://cert-manager.io/>`_) as well as a certificate issuer (see the `available issuers for cert-manager <https://cert-manager.io/docs/usage/issuer/>`_) already deployed in the Kubernetes cluster. *(Required if Ingress with TLS is enabled)*

- **Certificate Issuer** (``CertificateIssuer``):  
  The name of the certificate issuer to use for Firely Server when Ingress with TLS is enabled. Must be at least 4 characters long and can include alphanumeric characters, dots, and dashes. *(Required if Ingress with TLS is enabled)*

- **Service Type** (``ServiceType``):  
  The Kubernetes service type for exposing Firely Server. Choose from:
  
  - ``LoadBalancer``: Exposes the service externally using a cloud provider's load balancer. Typically used if not using Ingress.
  - ``ClusterIP``: Exposes the service on a cluster-internal IP. Must be used if Ingress is enabled.
  - ``NodePort``: Exposes the service on each node's IP at a static port.
  
  *(Required)*

- **License** (``License``):  
  The Firely Server license in JSON format. This field is required and must be a valid JSON object (starts and ends with curly braces). An evaluation license can be retrieved from `the Firely Portal <https://fire.ly/firely-server-trial/>`_.

- **appsettings** (``appsettings``):  
  The Firely Server application settings in JSON format. This field is required and must be a valid JSON object but you can specify ``{}``. You can find more details about the available settings in the :ref:`Firely Server Settings reference section <fs_settings_reference>`.

- **logsettings** (``logsettings``):  
  The Firely Server log settings in JSON format. This field is required and must be a valid JSON object but you can specify ```{}``. You can find more details about the available settings in the :ref:`Firely Server Settings logsettings section <configure_log>`.