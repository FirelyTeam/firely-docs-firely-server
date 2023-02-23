#!/bin/bash

docsroot=$(pwd)
echo "Building documentation in folder ${docsroot}"
docker run --rm -v ${docsroot}:/docs firely.azurecr.io/firely/docs-sphinx:latest
index="${docsroot}/_build/html/index.html"
echo "Build ready, showing output from ${index}"
open ${index} &2> /dev/null || xdg-open ${index} &2> /dev/null
echo "Done"
