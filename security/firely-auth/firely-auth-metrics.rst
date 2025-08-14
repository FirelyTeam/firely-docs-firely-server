.. _firely_auth_metrics:

OpenTelemetry
=============

Firely Auth can be configured to expose OpenTelemetry metrics. By using OpenTelemetry, Firely Auth enables observability into throughout its services based on an open standard. See `What is OpenTelemetry <https://opentelemetry.io/docs/what-is-opentelemetry/>`_ for an introduction to the standard.

Metrics
-------

Firely Auth emits a variety of runtime metrics, building on the Duende IdentityServer metrics. For a full description of the available IdentityServer metrics, see the `Duende IdentityServer OpenTelemetry documentation <https://docs.duendesoftware.com/identityserver/diagnostics/otel/>`_.
See :ref:`feature_opentelemetry` for options to distribute and consume metrics OpenTelemetry export/processing pipeline, or to visualise them in the Firely Performance Dashboard.