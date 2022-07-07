### Considerations
The task did not specify the way cities are validated when they are entered into the command line, 
thus I decided that the validation will check if there is at least one entered city in the Weather service that matches with the ones from API.
If there is no match - a message is displayed.
The weather forecast is shown only for the matched cities.
I added a simple available city cache if there are more available cities added. the cache TTL is currently set to 31 seconds.

### Requirements
1. App could become a web or desktop app:
The app logic was implemented in the Application layer that combines other layers: Infrastructure for the API and Persistence for the storage. I decided to skip Domain, since there is little domain logic in the app.
One could create an MCV webapi or a desktop app and use the Application project to reuse the logic.
2. API could change:
API was encapsulated in a client that can change as the dependency changes.
3. Persistent data storage could change
Persistence was encapsulated ina repository that can chaneg as persistence changes.

### Other
Contract mapping. Due to exactly the same contracts, I have decided to currently not create separate contract classes for each projects, however, it might be the case that
different dependencies (application, api, persitence) could have different models. In such case those models should be created and mapped from one layer to another.
The WeatherForecast model could be improved: temperature and precipitation could be decimal instead of int.
Some cities could have shell-sensitive names. Special characters should be escaped when passed as arguments. For Powershell one can use quotes for arguments for this reason.
Logging is not tested at the moment - Microsoft's ILogger cannot be easily created by Moq, so I didn't go into that rabbit hole. But one could spend more time on that and actually check that logging works in unit tests.
Infrastructure and Persistence should be covered with integration tests. However, I see value in also using unit tests here, since Flurl is testable. Also, persistence is currently in-memory, so there is no reason not to test it too.
There is a consistency issue in the ConsoleApp if the weather fetch does not return in 15 seconds. Currently the app will just pile up new Tasks, however, 
this can be improved by paralellizing the weather fetch, then actually defining on the requirements side what needs to be done when the weather is not fetched on time - 
maybe display a message and cut old tasks, then start new, or wait longer, etc.
Another thing that could be done - API calls could be retried on failure (since network calls are brittle). Polly could be used for that.