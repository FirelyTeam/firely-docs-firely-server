(fhiruserlookup)=

# FHIR User Lookup - $fhirUser-lookup

```{note}
The features described on this page are available in **all** {ref}`Firely Server editions <vonk_overview>`.
The `$fhirUser-lookup` plugin is distributed with Firely Server starting from version **6.7.0**.
```

## Overview

The `$fhirUser-lookup` custom operation allows {ref}`Firely Auth <firely_auth_index>` to delegate the lookup of a FHIR user (Patient or Practitioner) to Firely Server, rather than performing a plain system-level search itself.

Before Firely Auth 4.6.0, resolving the `fhirUser` claim during {ref}`SSO login <firely_auth_sso>` was done entirely inside Firely Auth by executing a system-level search against Firely Server using name, email, or other parameters derived from the identity provider's claims. This approach had limitations:

- The search results might not be distinct enough to identify a single user.
- The search could not make use of additional data that was available in the SSO claims but not directly mapped to any property on a resource, or a property not covered by a search parameter.

Starting with **Firely Auth 4.6.0**, Firely Auth checks the server's `CapabilityStatement` for the `$fhirUser-lookup` operation. If the operation is advertised, Firely Auth forwards the lookup to the server via this operation. If the operation is *not* present in the `CapabilityStatement`, Firely Auth falls back to the previous plain search behavior.

## How it works

The high-level flow when `$fhirUser-lookup` is enabled depends on how the user signs in.

### SSO users

1. Firely Auth receives claims from the external identity provider.
2. The `FhirUserLookupClaimsMapping` configuration in Firely Auth converts the relevant claims into FHIR search parameters (see {ref}`firely_auth_settings_externalidp`).
3. Firely Auth calls `POST [base]/$fhirUser-lookup` on Firely Server, passing a `Parameters` resource containing the search parameters.
4. Firely Server (via the plugin described below) converts those parameters into a system-level search and returns the matching resource.
5. Firely Auth derives the `fhirUser` claim from the resource type and ID of the returned resource.

### Locally created users

1. Firely Auth uses the locally stored user information, such as name and email, to construct FHIR search parameters.
2. Firely Auth calls `POST [base]/$fhirUser-lookup` on Firely Server, passing a `Parameters` resource containing those search parameters.
3. Firely Server (via the plugin described below) converts those parameters into a system-level search and returns the matching resource.
4. Firely Auth derives the `fhirUser` claim from the resource type and ID of the returned resource.

The built-in plugin performs a straightforward conversion of the `Parameters` resource into a system-level search — matching the previous search behavior exactly. For advanced scenarios (e.g. MPI look-ups) you can replace this with a custom plugin; see [Custom implementations](#custom-implementations) below.

## Configuration

### Enable the $fhirUser-lookup plugin

Ensure the plugin namespace is included in the `PipelineOptions` section of the {ref}`appsettings <configure_appsettings>`:

```json
"PipelineOptions": {
    "PluginDirectory": "./plugins",
    "Branches": [
        {
            "Path": "/",
            "Include": [
                "Vonk.Plugin.FhirUserLookupOperation.FhirUserLookupConfiguration"
            ]
        }
    ]
}
```

Once the plugin is loaded, the operation will be advertised in the server's `CapabilityStatement` and Firely Auth 4.6.0+ will automatically use it.

```{note}
No license token is required for this plugin.
```

### Verify the operation is listed

Confirm that `$fhirUser-lookup` appears in the `CapabilityStatement` returned by `GET [base]/metadata`. Firely Auth uses this check to decide whether to invoke the operation or fall back to a direct search.

## Custom implementations

The built-in plugin covers the standard use case. For scenarios that require additional processing during the user look-up — such as calling an external Master Patient Index (MPI) before searching Firely Server — you can implement a custom plugin that handles the `$fhirUser-lookup` operation.

## See also

- {ref}`firely_auth_sso` — configuring `FhirUserLookupClaimsMapping` in Firely Auth
- {ref}`firely_auth_settings_externalidp` — full reference for the External Identity Providers settings
- {ref}`vonk_plugins_fhiruserlookup` — plugin entry in the Available Plugins reference
