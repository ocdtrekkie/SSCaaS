# Sandstorm Cron as a (Windows) Service

SSCaaS combines my two true loves: Sandstorm.io and badly-written Visual Basic code. Specifically, this implements a Windows service on a PC that periodically pokes a Sandstorm app API a few times. It's mostly intended for coercing TinyTinyRSS to update it's feeds.

## Installation Instructions

- Right-click the zip file, click Properties, and "Unblock" the folder.
- Unzip the release folder into a permanent location.
- Edit the `SSCaaS.exe.config` with the Sandstorm API URL and credentials needed to access your Sandstorm grain.
- Run `install-sscaas.bat` as admin to add the SSCaaS service to your machine.
- Run `net start SSCaaS` to launch the service.

### NOTE: The defaults are set to the point which appear to be functionally useful, whilst not causing significant burden on a Sandstorm server. I strongly discourage significantly modifying the variables unless it is your own Sandstorm server and you know what you are doing.

## Using SSCaaS with the XRF Sync Module

SSCaaS can also be used as a lightweight client to send heartbeat messages to XRF's sync module.

- Set the URL in `SSCaaS.exe.config` to `http://SYNC_SERVER_DOMAIN_HERE/v1.php?message_type=heartbeat&amp;destination=server&amp;access_key=ACCESS_KEY_HERE`.
- Username and password are not required to connect to XRF Sync, so their contents are irrelevant.
