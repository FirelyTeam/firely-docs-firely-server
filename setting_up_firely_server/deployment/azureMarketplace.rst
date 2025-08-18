.. _deploy_azure_marketplace:

Using Firely Server from Azure Marketplace
==========================================

Firely Server is available as a Kubernetes App in the Azure Marketplace, which allows you to deploy it easily on Azure Kubernetes Service (AKS) with a single click. 
The Azure Marketplace deployment is ideal for users who want to quickly get started with Firely Server on Azure without needing to manually 
configure Kubernetes resources.

Concretely, when deploying Firely Server from the Azure Marketplace, an AKS extension of type ``Firely.FirelyServerEssentialsNonUs`` is deployed to your existing cluster, and the 
extension consists in a set of Kubernetes resources packaged as a Helm chart and deployed in the ``firely-market-place`` namespace. 

Pre-requisites
--------------

Before deploying Firely Server from the Azure Marketplace, ensure you have the following:

- An Azure account with an active subscription.
- An Azure Kubernetes Service (AKS) cluster where you want to deploy Firely Server (see for example `this tutorial <https://learn.microsoft.com/en-us/azure/aks/tutorial-kubernetes-deploy-cluster>`_).
- Basic knowledge of Azure and Kubernetes concepts.
- A valid Firely Server license in JSON format. An evaluation license can be retrieved from `the Firely Portal <https://fire.ly/firely-server-trial/>`_.


In addition, if you want to use Ingress with TLS, you need to have the following: 

- A registered hostname (i.e. a CNAME or A record in a DNS provider). 
- A certificate manager (typically `cert-manager <https://cert-manager.io/>`_). 
- A certificate issuer (see the `available issuers for cert-manager <https://cert-manager.io/docs/usage/issuer/>`_) already deployed in the Kubernetes cluster.
- An Ingress Controller already deployed in your Kubernetes cluster. 
  If you don't have one, you can deploy the `NGINX Ingress Controller <https://learn.microsoft.com/en-us/azure/aks/ingress-basic>`_ or the `Traefik Ingress Controller <https://learn.microsoft.com/en-us/azure/aks/ingress-tls>`_.

In order to simplify the setup process, we have created a `public repository <https://github.com/FirelyTeam/kubernetes-cluster-deployment>`_ with sample code to deploy an Azure Kubernetes cluster with Terraform 
as well as deploying an ingress Controller and a certificate manager using Helm.

Deployment Steps
-----------------

In order to deploy Firely Server from the Azure Marketplace, follow these steps:    

1. Go to the `Azure Marketplace <https://azuremarketplace.microsoft.com/en-us/marketplace/apps?search=Firely&page=1>`_ and search for "Firely Server".
2. Click on the "Get it now" button to start the deployment process.
3. On the first panel, provide the Azure subscription, the resource group containing the kubernetes cluster where you want to deploy the Firely Server extension and then select the kubernetes cluster where the Firely Server extension should be deployed.
4. On the second panel, provide the parameters for configuring the deployment (see below for details). 
5. Click "Review + Create" to review your settings and then click "Create" to deploy Firely Server.

.. note::
  For the most basic deployment, you can simply provide a license, and leave the default values for all other parameters.
  This will deploy Firely Server in the selected kubernetes cluster as a kubernetes deployment and create a load balancer with a public IP, exposing Firely Server on port 80. 
  The public IP addresss exposed by the load balancer can be retrieved from the Azure portal by inspecting the ``Services and Ingresses`` section under ``Kubernetes resources`` of the kubernetes cluster.
  Unfortunately, with this setup, the Firely Server UI will not be displayed correctly as it expects to be accessed via HTTPS, and the load balancer does not provide TLS termination.

For more advanced scenarios, you can customize the deployment by providing additional parameters as described below.

You can monitor the deployment progress in the Azure portal under the "Deployments" section of the selected resource group.

.. note::
  In order to delete the deployment, you can either delete the extension in the Azure Portal 
  (the extensions are located in the ``Extensions + applications`` in the ``Settings`` section of the AKS instance where the extension is deployed) or
  delete the deployment (the deployment is located in the ``Deployments`` section of the resource group where the extension is deployed).



Parameters
----------

The following parameters are available for configuring the Firely Server deployment:

- **Extension Resource Name** (``extensionResourceName``):  
  The name for the extension resource. This must be unique for the cluster, must only contain alphanumeric characters and dashes, and the value must be between 6 and 40 characters long, we recommend keeping the default ``firely-server-market-place``. *(Required)*

.. note::
    There could be at most one extension resource of type ``Firely.FirelyServerEssentialsNonUs`` per AKS cluster. If you try to deploy again the Firely Server offer with with the different extension resource name, the deployment will fail. If you re-use the same name, the existing extension will be updated with the new parameters.

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
  
  *(Required)*

- **License** (``License``):  
  The Firely Server license in JSON format. An evaluation license can be retrieved from `the Firely Portal <https://fire.ly/firely-server-trial/>`_.

- **appsettings** (``appsettings``):  
  The Firely Server application settings in JSON format. This field is required and must be a valid JSON object but you leave the default value of ``{}``. You can find more details about the available settings in the :ref:`settings section <fs_settings_reference>`.

- **logsettings** (``logsettings``):  
  The Firely Server log settings in JSON format. This field is required and must be a valid JSON object but you can leave the default value of ``{}``. You can find more details about the available settings in the :ref:`log settings section <configure_log>`.

.. note::
  In order to update the parameters, you can either redeploy the extension with the new parameters or update the extension parameters in the Azure Portal 
  (the extensions are located in the ``Extensions + applications`` in the ``Settings`` section of the AKS instance where the extension is deployed). If updating the extension parameters,
  you need to encode the JSON values in base64 format.


