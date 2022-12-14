# Plan and Estimation:
## Part 1: Data Analysis and API Design
### Source Data
The data source contains CSV files named by date, containing samples like this.
|FIPS|Admin2|Province_State|Country_Region|Last_Update        |Lat     |Long_    |Confirmed|Deaths|Recovered|Active|Combined_Key|Incident_Rate     |Case_Fatality_Ratio|
|----|------|--------------|--------------|-------------------|--------|---------|---------|------|---------|------|------------|------------------|-------------------|
|    |      |              |Afghanistan   |2021-01-02 05:22:33|33.93911|67.709953|52513    |2201  |41727    |8585  |Afghanistan |134.89657830525067|4.191343095995277  |

#### Data Transformation
|Date      |Region        |Confirmed|Deaths|Recovered|Active|Incident_Rate     |Case_Fatality_Ratio|
|----------|--------------|---------|------|---------|------|------------------|-------------------|
|2021-01-02|Afghanistan   |52513    |2201  |41727    |8585  |134.89657830525067|4.191343095995277  |

|Date     |Region        |Confirmed|Deaths|Recovered|Active|Incident_Rate     |Case_Fatality_Ratio       |
|---------|--------------|---------|------|---------|------|------------------|--------------------------|
|file name|Region        |SUM      |SUM   |SUM      |SUM   |AVG               |SUM(Deaths)/SUM(Confirmed)|
 
Note: Incident_Rate is defined as cases per 100,000 persons, so safe to take the average.

#### Dropped field and reason:
- Combined_Key: Key used by the source before aggregation, useless after our transformation is applied.
- FIPS|Admin2|Province_State|Lat|Long_: Geolocation-related data, Irrelevant after aggregated by Country_region.

## API Design
### RESTful API
GET /daily_case_summary?Region=Afghanistan&StartDate=2021-01-02&EndDate=2021-01-02
 - Params StartDate & EndDate inclusive
200 Response Shape:
```
[
    {
        "Region": "Afghanistan",
        "Date": "2021-01-02",
        "Confirmed": 52513,
        "Deaths": 2201,
        "Recovered": 41727,
        "Active": 8585,
        "Incident_Rate": 134.89657830525067,
        "Case_Fatality_Ratio": 4.191343095995277
    }
]
```
* Use OpenAPI doc for documentation.

## Initial pick of Tech Stack
DB: Sqlite - for simplicity.
.Net Core 7 - slightly interested in their recent claim on .Net 7 performance

## Tasks and estimations:
- Setup and configure development environment (Setup & study .Net Core runtime and C# package manager etc.) ~ 1-4 hr
- Study some C# .Net SDK basics & .Net Core basics (C# Console Hello World & Asp .net core Hello World). ~ 1~2 hr
- Code the Data Transformation logic. ~ 15 mins (sort by Region then iterate over each row, easy).
- Study and pick an OpenApi implementation and a CSV parser, and scaffold the application ~ 3-6 hr.
- Containerize using docker ~ 30 mins
- Update the readme file for clear setup instructions ~ 30 mins - 1hr

## Total Estimation:
6 hr - 14 hr