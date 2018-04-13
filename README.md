# FM Data

A C# client library for the FileMaker 16 REST (Data) API.

| Build Status | Activity | Nuget | License |
|---|---|---|---|
|[![filemaker MyGet Build Status](https://www.myget.org/BuildSource/Badge/filemaker?identifier=656ddd5c-8aca-4d86-ba00-1508dd519999)](https://www.myget.org/feed/filemaker/package/nuget/FMData) | [![FMData repository/commit activity](https://img.shields.io/github/commit-activity/w/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/commits/master) | (coming soon) | [![license](https://img.shields.io/github/license/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/blob/master/LICENSE) |

## FileMaker REST / Data API Documentation

- [FileMaker REST API Documentation](https://fmhelp.filemaker.com/docs/16/en/restapi/)

## Example Usage

    using(var client = new FMDataClient(server, user, pass, layout))
    {
        // TODO: write usage docs
    }

## Planned Features

- Support for strongly typed requests and responses
- Additional operations from Data API
  - [Create record](https://fmhelp.filemaker.com/docs/16/en/restapi/#work-with-records_create-record)
  - [Update record](https://fmhelp.filemaker.com/docs/16/en/restapi/#work-with-records_edit-record)
  - [Delete record](https://fmhelp.filemaker.com/docs/16/en/restapi/#work-with-records_delete-record)
  - [Range of Records](https://fmhelp.filemaker.com/docs/16/en/restapi/#work-with-records_get-records)
  - [Finds with paging with range and offset parameters and portals](https://fmhelp.filemaker.com/docs/16/en/restapi/#perform-find-requests)
  - [Global fields](https://fmhelp.filemaker.com/docs/16/en/restapi/#set-global-fields)
- Batch operations

## Repository Statistics

[![FMData repository/commit activity the past year](https://img.shields.io/github/commit-activity/y/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/commits/master)

[![FMData issues](https://img.shields.io/github/issues/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/issues)

[![Code size in bytes](https://img.shields.io/github/languages/code-size/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/commits/master)

[![Language Count](https://img.shields.io/github/languages/count/fuzzzerd/fmdata.svg?style=flat-square)](https://github.com/fuzzzerd/fmdata/commits/master)

## License

[MIT License](https://github.com/fuzzzerd/fmdata/blob/master/LICENSE)
