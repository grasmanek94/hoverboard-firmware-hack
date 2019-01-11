#include <ESP8266WiFi.h>
#include <SoftwareSerial.h>

#define MAX_SRV_CLIENTS 1
const char* ssid = "GZX";
const char* password = "";

SoftwareSerial ss(5,4);

WiFiServer server(23);
WiFiClient serverClients[MAX_SRV_CLIENTS];

struct SerialCommand
{
    int16_t steer;
    int16_t speed;
} __attribute__ ((packed));

SerialCommand command;

void setup() 
{
    ss.begin(19200);
    Serial.begin(19200);
    
    WiFi.begin(ssid, password);
    
    command.steer = 0;
    command.speed = 0;
}

int current = 0;
char data[5];

int wifi_current = 0;
char wifi_data[5];

bool old_wifi_status = false;

unsigned long time_x = 0;

void loop() 
{  
    String str_dat = "";
    unsigned long time_y = millis();
    if(time_y - time_x > 50)
    {
        //str_dat = "S" + String(command.speed) + ",T" + String(command.steer) + "\r\n";
        time_x = time_y;
        Serial.print(str_dat);
        ss.write((char*)&command, sizeof(command));
    }

//////////////////////////////////////////////////

    while(Serial.available())
    {
        char c = Serial.read();
        if(c == ';')
        {
            data[0] = ';';
            current = 1;
        }
        else if(c != ';')
        {
            data[current++] = c;
            if(current == 5)
            {
                current = 0;
                memcpy((char*)&command, &data[1], 4);
            }
        }
    }
    
//////////////////////////////////////////////////

    bool connected_wifi = WiFi.status() == WL_CONNECTED;

    if(old_wifi_status != connected_wifi)
    {
        // smth changed
        old_wifi_status = connected_wifi;

        if(connected_wifi)
        {
            server.begin();
        }
        else
        {
            for(int i = 0; i < MAX_SRV_CLIENTS; i++)
            {
                if (serverClients[i] && serverClients[i].connected())
                {
                    serverClients[i].stop();
                }
            }
            server.stop();
        }
    }

    if(connected_wifi)
    {
        // check if there are any new clients
        if (server.hasClient())
        {
            bool added = false;
            for(int i = 0; i < MAX_SRV_CLIENTS; i++)
            {
                //find free/disconnected spot
                if (!serverClients[i] || !serverClients[i].connected())
                {
                    if(serverClients[i])
                    {
                        serverClients[i].stop();
                    }
                    serverClients[i] = server.available();
                    added = true;
                    break;
                }
            }

            if(!added)
            {
                // no free/disconnected spot so reject
                WiFiClient serverClient = server.available();
                serverClient.stop();
            }
        }
        
        //check clients for data
        for(int i = 0; i < MAX_SRV_CLIENTS; i++)
        {
            if (serverClients[i] && serverClients[i].connected())
            {
                while(serverClients[i].available()) 
                {
                    char c = serverClients[i].read();
                    if(c == ';')
                    {
                        wifi_data[0] = ';';
                        wifi_current = 1;
                    }
                    else if(c != ';')
                    {
                        wifi_data[wifi_current++] = c;
                        if(wifi_current == 5)
                        {
                            wifi_current = 0;
                            memcpy((char*)&command, &wifi_data[1], 4);
                        }
                    }                    
                }
            }
        }
        
        //check UART for data
        if(str_dat.length())
        {
            for(int i = 0; i < MAX_SRV_CLIENTS; i++)
            {
                if (serverClients[i] && serverClients[i].connected())
                {
                    serverClients[i].write(str_dat.c_str(), str_dat.length());
                    delay(1);
                }
            }
        }      
    }
}
