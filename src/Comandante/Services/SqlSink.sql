CREATE TABLE ComandanteLogs
(
	Id NVARCHAR(8000),
	TraceIdentifier NVARCHAR(8000),
	LogLevel NVARCHAR(8000),
	EventId NVARCHAR(8000),
	LoggerName NVARCHAR(8000),
	Details NVARCHAR(8000),
	Exception NVARCHAR(8000),
	Created DATE,
)

CREATE TABLE ComandanteRequests
(
	TraceIdentifier NVARCHAR(8000),
	Host NVARCHAR(8000),
	Method NVARCHAR(8000),
	Scheme NVARCHAR(8000),
	Protocol NVARCHAR(8000),
	Path NVARCHAR(8000),
	PathBase NVARCHAR(8000),
	Headers NVARCHAR(8000),
	Cookies NVARCHAR(8000),
	Form NVARCHAR(8000),
	QueryString NVARCHAR(8000),
	Body NVARCHAR(8000),
	IdentityName NVARCHAR(8000),
	MaxLogLevel NVARCHAR(8000),
	ResponseStatusCode NVARCHAR(8000),
	ResponseHeaders NVARCHAR(8000),
	Created DATE,
	Completed DATE
)
