
Mongo

```bash
docker run -d -p 27017:27017 --name shopping-mongo mongo:5.0

docker exec -it shopping-mongo /bin/bash

docker run -d -p 3000:3000 mongoclient/mongoclient

docker run -d -p 6379:6379 --name aspnetrun-redis redis

docker exec -it aspnetrun-redis /bin/bash
```

```bash
docker-compose -f .\docker-compose.yml -f .\docker-compose.override.yml up -d
```


After that, we are able to run mongo commands. 
Let me try with 

 - create database
 - create collection
 - add items into collection
 - list collection


```C#
ls
mongo
show dbs
use CatalogDb  --> for create db on mongo
db.createCollection('Products')  --> for create people collection

db.Products.insertMany([{ 'Name':'Asus Laptop','Category':'Computers', 'Summary':'Summary', 'Description':'Description', 'ImageFile':'ImageFile', 'Price':54.93 }, { 'Name':'HP Laptop','Category':'Computers', 'Summary':'Summary', 'Description':'Description', 'ImageFile':'ImageFile', 'Price':88.93 } ])


db.Products.find({}).pretty()
db.Products.remove({})

show databases
show collections
db.Products.find({}).pretty()
```