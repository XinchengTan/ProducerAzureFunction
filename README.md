# ProducerAzure
This project simulates a data record producer in a producer-consumer framework.  
It generates data record by user specification in an input configuration json string.
For example, users can specify the format of the data records to generate (e.g. field name, data type, etc.) as well as other parameters, such as error rate and total records.

As an Azure Function, the main program is triggered by any HTTP request with a json-formatted config in its request body. Once triggered, it parses the config json and send generated data to targeted consumer.


## Input Configuration
The required configuration for this project:
![alt text](https://github.com/XinchengTan/ProducerAzureFunction/blob/master/Misc/FullConfig_eg.png)


## Execution Instruction

### Run on Azure
Currently, the project is deployed via Azure Function Apps on a personal Azure portal.
Its Function URL is https://teamaproducer.azurewebsites.net/api/ProducerAzure?code=PzwU10CYYXZx5LO1v8uDbjQqJe1rljBts8rqGOGxlVMhnyBLpU5e6Q==
It accepts config JSON sent as an HTTP request and echos the config in the response. However, it does not send data to any consumer system yet.


### Run Locally
1. Clone this project to a local directory
2. Open ProducerAzure.sln and run ProducerAzure.cs, and copy the localhost IP address shown on Terminal
3. Install Postman
4. Create an HTTP POST request in Postman
   -- In Request header, add "Content-Type" as KEY and "application/json" as VALUE
   -- Request body is the json-formatted configuration
   -- Paste the locahost address into request URL
5. Click Send on Postman and you can monitor the program on Terminal
