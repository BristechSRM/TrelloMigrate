# TrelloMigrate
## Notes
1. Currently, TrelloMigrate must be run on a system with clean databases.
2. To run TrelloMigrate, you must have a Trello account, and you must be a member to the BristechSrm Trello Board.
3. If there are any detected data errors on the board, the migration will throw an exception and cancel before any import is performed. 

## Setup
To setup TrelloMigrate for running, edit App.config and set SessionsServiceUrl and CommsServiceUrl to point to the correct sessions service and comms service.

Next, copy the TrelloCredsTemplate.config and rename it to TrelloCreds.config.
Login to Trello with your account that is connected to the BristechSrm board.

Now navigate to <https://trello.com/app-key>. In trelloCreds, set the TrelloKey value to the Key field on that page. Next, click the link at the end of the first paragraph to generate a Token. Click allow and then copy the generated token to the TrelloToken field of the TrelloCreds file.

Setup is now complete. The TrelloCreds is git ignored, so you don't need to worry about it getting checked in (Unless you name it wrong or do something weird)

## Run
Nothing special needs to be done. Build and run with Visual Studio, or using the scripts.

./setup.sh (only needs to be run once. This pulls Nuget and Fake)

./build.sh

./run.sh
