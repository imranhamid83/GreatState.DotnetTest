
# API to retrieve and create pages on CMS

This is a simple API which retrieves the pages from CMS based on the role requested in the http cal

# assumptions
Each request will provide a role and the key. The key is attached to the role int he back-end cms and will be validated for the authentication and authorization of each request. 

Only staff and admin is allow to make CRUD requests
Anonymous role can do read requests but must provide key too.

Following roles will be created in Sitecore
sitecore/anonymous 
sitecore/admin 
sitecore/staff

# Not able to do
Add logging for requests

# Uploaded two documents in the root folder
I have added the two documents on the root in response to the following two tasks:
•	Propose a simple architecture for handling high traffic and CMS sync. Could be a markdown diagram or short doc.
•	Suggest how this might be deployed in Azure to scale, be monitored, and be secure (document, not implement).



