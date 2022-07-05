#!/bin/bash

docsroot=$(pwd)
echo "Building documentation in folder ${docsroot}"
docker run --rm -v ${docsroot}:/docs firely.azurecr.io/firely/docs-sphinx:latest
sudo chown -R ${USER}:${USER} ${docsroot}/_build
index="${docsroot}/_build/html/index.html"
echo "Build ready, showing output from ${index}"
firefox ${index}
echo "Done"
