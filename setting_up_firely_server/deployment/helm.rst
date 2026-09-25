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

.. _deploy_helm_envvar:

Starting Firely Server with environment variables
-------------------------------------------------

If you have your configuration in a file, e.g. ``firely-server.env`` exported by the Guided Setup, you can store it as a Kubernetes Secret and let the Helm chart pass all its entries to the Firely Server container as environment variables. See :ref:`configure_envvar_file` for the format of the file.

#. Create the Secret from the env file in the namespace where you deploy Firely Server:

   .. code-block:: bash

      kubectl create secret generic firely-server-env \
        --namespace <namespace> \
        --from-env-file=./firely-server.env

   Each ``NAME=value`` line becomes one entry in the Secret. Quotes are not removed, so do not put quotes around the values.

#. Refer to the Secret in your ``values.yaml`` with the ``envFromSecret`` parameter of the chart:

   .. code-block:: yaml

      envFromSecret: "firely-server-env"

#. Install or upgrade the release as described in the `Firely Server chart README <https://github.com/FirelyTeam/Helm.Charts/blob/main/charts/firely-server/README.md>`_.

If the env file only holds non-confidential settings, you can use a ConfigMap (``kubectl create configmap ... --from-env-file``) together with the ``envFromConfigMap`` parameter instead.

Restarting after a change to the env file
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Kubernetes sets the environment variables of a container only when the pod starts. Updating the Secret does **not** restart the running pods, and ``helm upgrade`` with an unchanged ``values.yaml`` doesn't either. After you change ``firely-server.env``:

#. Replace the content of the Secret with the new file:

   .. code-block:: bash

      kubectl create secret generic firely-server-env \
        --namespace <namespace> \
        --from-env-file=./firely-server.env \
        --dry-run=client -o yaml | kubectl apply -f -

   This replaces all entries, so variables that you removed from the file are removed from the Secret as well.

#. Restart the Firely Server pods, so they pick up the new values:

   .. code-block:: bash

      kubectl get deployments --namespace <namespace>
      kubectl rollout restart deployment/<firely-server-deployment> --namespace <namespace>
      kubectl rollout status deployment/<firely-server-deployment> --namespace <namespace>

   A rollout restart replaces the pods one by one. With more than one replica, Firely Server stays available during the restart.
