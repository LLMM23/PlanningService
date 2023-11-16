#!/bin/bash

export VAULT_ADDR='http://vault:8200'
export VAULT_TOKEN='00000000-0000-0000-0000-000000000000'

# give some time for Vault to start and be ready
sleep 10

# Insert 2 secrets  
vault kv put secret/database ConnectionString=mongodb://admin:1234@mongodb-dev:27200/?authSource=admin
vault kv put secret/authentication Secret=fwnhy8423HBgbirfffwefefwefwefwedqwsad6q3wfrhgedr32etsg7u Issuer=QuickGoTaxi Salt='$2a$11$NnQ3D9KHpPr1UOjTo/2fXO'