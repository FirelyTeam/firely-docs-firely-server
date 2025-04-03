.. _deploy_helm:

===================================================
Using Firely Server on Kubernetes with a Helm chart
===================================================

It is very common to run Firely Server on a Kubernetes cluster, and we provide a Helm chart to simplify the deployment process. 
The chart is open source and can be found in the `FirelyTeam/Helm.Charts <https://github.com/FirelyTeam/Helm.Charts>`_ repository.

The Helm chart can be used to deploy Firely Server to any Kubernetes cluster, including Azure Kubernetes Service (AKS), Amazon Elastic Kubernetes Service (EKS), Google Kubernetes Engine (GKE), and on-premises clusters.
The only requirement is that the Kubernetes version must be 1.19.0 or higher.

The chart is designed to be flexible and can be customized to fit your specific needs. 
The deployment instructions, as well as the configuration options, are described in detail in the `Firely Server chart README <https://github.com/FirelyTeam/Helm.Charts/blob/main/charts/firely-server/README.md>`_.

The same repository also contains a Helm chart to deploy :ref:`Firely Auth <feature_accesscontrol_idprovider>`. The corresponding deployment instructions and settings are described in the `Firely Auth chart README <https://github.com/FirelyTeam/Helm.Charts/blob/main/charts/firely-auth/README.md>`_.