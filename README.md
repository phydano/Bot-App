# Bot-App
Advanced Training Bot Application for Contoso Bank Limited.

### Business Justification:

With the Conversational Bot I created, the users can now have some interactions with the bot (through Card, Prompts). The user can registered (POST) to the database, and they can check the balance (GET) or deposit the money (UPDATE) or even remove themselves (DELETE). Furthermore the bot provides an exchange rate feature where users can ask the bot to do the exchange (E.g. NZD to USD) using Yahoo api. The application also used the Text Analytics Cognitive service to recognized the feedback given back from the users to check the state of the sentiment between 0-1. With this we will know whether the users like Bot service or not. It also remembers the name of the registed user who is in that channel. The user can also get additional help through external URL that allows them to ask the support team directly if the Bot does not meet their needs. 

### How to communicate with the Bot App:

- For Deposit: Type 'Deposit' to your account number (E.g. Deposit $10 to RX12345).
- For Balance: Type 'Balance' on your account number (E.g. Balance on RX12345).
- For Register: Type 'Register' (E.g. Register me to the bank).
- For De-register: Type 'Remove' your account number (E.g. Remove RX12345).
- For Currency Conversion: Type 'CurrencyX' to 'CurrencyY' (E.g. NZD to USD).
- For additional Help: Type 'Help". 
- Any other commands, the bot will only reply the steps on 'How to Use the Bots'. 

### Assumption of the Application:

The following assumptions are made while developing the bot app:
- The users have their indentity identified somehow before they can register to the bank. 
- To deposit their money, there is some sort of transaction taking place. For example, transfer the money in from another services (Paypal / Bank transfer).
- The users are soft-delete from the system, to fully remove themselves from the bank they must visit the bank.
- The feedback given by the users are sent to the team who will use this feedback to improve on the service. (The Bot app currently does not send the comment anyway - it just evaluate the comment through Text Analytics) 

