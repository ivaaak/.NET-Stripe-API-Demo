## Create a new customer
First we have to add the customer to Stripe in order to create a charge on their credit card. 
Below is a payload I will submit through a POST request:

{
  "email": "yoourmail@gmail.com",
  "name": "Christian Schou",
  "creditCard": {
    "name": "Christian Schou",
    "cardNumber": "4242424242424242",
    "expirationYear": "2024",
    "expirationMonth": "12",
    "cvc": "999"
  }
}

When testing out the Stripe API you got three card numbers to test on. You can set expiration and CVC to what you prefer.

4242424242424242 (Successful Payments)
4000000000009995 (Failed Payments)
4000002500003155 (Required Authentication)



## Create Payment/Charge on Customer

{
  "customerId": "cus_MgWyoA3VnOozzn",
  "receiptEmail": "christian.schou17@gmail.com",
  "description": "Demo product for Stripe .NET API",
  "currency": "USD",
  "amount": 1000
}