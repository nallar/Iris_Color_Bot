
1 - cd to /ColorBot

2 - build the release:

`dotnet build -c Release`

3 - publish the release

`dotnet publish -c Release`

3 - build the docker image:

`docker build -t irisbotimage -f dockerfile .`

4 - create a container:

`docker create --name iris-bot irisbotimage`



helpful commands:

explore a docker image:

`docker run --rm -it --entrypoint=/bin/bash name-of-image`

explore a (running!) docker container:

`docker exec -it name-of-container bash`
