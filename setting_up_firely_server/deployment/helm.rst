.. _deploy_helm:

===================================================
Using Firely Server on Kubernetes with a Helm chart
===================================================

It is very common to run Firely Server on a Kubernetes cluster. This can be any Kubernetes implementation, it is not bound to a specific cloud provider.

To ease the process we provide a helm chart to deploy Firely Server on the cluster. The chart can be found in the `FirelyTeam/Helm.Charts <https://github.com/FirelyTeam/Helm.Charts>`_ repository.

All the settings are described in the `Read.me <https://github.com/FirelyTeam/Helm.Charts/tree/main/firely-server>`_.

The same repository also contains a helm chart to deploy :ref:`Firely Auth <feature_accesscontrol_idprovider>`, but that is very early still. Documentation and settings will be added to that as well.
