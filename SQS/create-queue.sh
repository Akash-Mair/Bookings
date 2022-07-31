#!/bin/bash
echo "configuring sqs"
echo "==================="
awslocal --endpoint-url=http://localhost:4566 sqs create-queue --queue-name reservation-queue