REM You can install sphinx-autobuild using 'py -m pip install sphinx-autobuild'
REM Serving the generated documents on localhost:7000, this can be changed using the -p parameter

py -m sphinx -b html . .\_build\html
