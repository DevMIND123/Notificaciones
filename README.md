Para entrar dentro de cualquier contenedor:
docker exec -it <nombre-del-contenedor> bash

Una vez dentro del contenedor para entrar a la base de datos:
psql -U user -d habitosdb -> ejemplo con base de habitos y retos

para agregar los eventos a kafka manualmente:
docker exec -it retochimba-kafka bash  -> entrar al contenedor
kafka-topics --create --topic embarazo-registrado --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 
kafka-topics --create --topic ciclo-registrado --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 
kafka-topics --create --topic usuario-creado --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1 

