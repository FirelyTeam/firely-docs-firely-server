.. _deployment:

Firely Server deployment options
================================

You have several options for the deployment of Firely server. Next to the possibility of installing Firely Server locally it is possible to deploy using docker, Kubernetes, Azure and more. The pages below will guide you in the deployment process of your preferred method. 
If you are not sure what would be the best option for your use case, or if you have questions, you can always reach out to us at server@fire.ly.

.. image:: ../../images/FirelyDeployment.png
  :align: right
  :width: 250px
  :alt: Illustration of Firely server

.. toctree::
   :maxdepth: 2
   :titlesonly:
   :hidden:

   Binaries <binaries>
   Docker <docker>
   Azure App Service <azureWebApp>
   Kubernetes / Helm <helm>
   Reverse Proxy <reverseproxy/reverseProxy>
   HTTP(S) and Port settings <hosting>
   cors

.. rubric:: Local deployment

**Advantages:**

- Quick setup for development or evaluation purposes.
- Ideal for developers testing Firely Server on their local machine.
- No dependency on external infrastructure or orchestration tools.

.. seealso:: 
   See :ref:`use_binaries`

------

.. rubric:: Docker

**Advantages:**

- Simple and reproducible deployment using Docker images.
- Minimal setup required.
- Easily integrates with CI/CD pipelines and container registries when integrating Firely Server into existing products and workflows.

.. seealso:: 
   See :ref:`use_docker`

------

.. rubric:: Azure App Service

**Advantages:**

- Fully managed hosting with built-in scaling, patching, and monitoring.
- Easy deployment of Firely Server via zipfile with minimal DevOps overhead.

.. seealso:: 
   See :ref:`azure_webapp`

------

.. rubric:: Kubernetes (with Helm chart)

**Advantages:**

- Scalable deployment across multiple nodes for high availability.
- Declarative configuration using Helm for easier version control and automation.
- Ideal for enterprise environments with existing Kubernetes infrastructure.
- Compatible with all clouds (AKS, EKS, GKE) and on-prem clusters.

.. seealso:: 
   See :ref:`deploy_helm`
