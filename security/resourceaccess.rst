.. _feature_resourceaccessdefinition:

Resource Access
===============

This configuration is closely related to :ref:`multi tenancy <feature_multitenancy>` and :ref:`SMART on FHIR <feature_accesscontrol_config>`, and affects how those authorization methods process requests.
It allows to alter whether the authorization or multi-tenancy should allow access to specific resource types to unauthenticated users.


.. warning:: 

    Exercise caution when configuring this setting, as improper use may expose sensitive data.


    .. code-block:: json

            "ResourceAccess": {
                "Default": {
                  "Priority": 100,
                  "Tenant": "Required",
                  "Authorization": "Required"
                },
                "<unique-key>": {
                  "Priority": 200,
                  "Tenant": "Shared"
                  "Authorization": "None"
                  "Profiles": [],
                  "ResourceTypes": ["Practitioner", "Organization"],
                  "Filter": "_tag=vip"
                }
            }
            

:Priority: Will be used to choose most important logic entry if multiple entries would match the request.
:Tenant: Specifies if the tenant is required to be specified when accessing the resource, or should we use internally a ``shared`` tenant. It has to be either ``Required`` or ``Shared``.
:Authorization: Specifies if the authorization is required when accessing the resource. It has to be either ``Required`` or ``None``.
:Filter: Specifies a filter that resources must fulfill.
:Profiles: A list of ``meta.profile`` values to look for in the resource. The resource should have at least one of those values.
:ResourceTypes: A list of resource types this configuration will be applicable to. If left empty, will not match any resource type.