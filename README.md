# Data Extractor

A digital location database created for Beyond Skyrim: Cyrodiil. Built with React, CSS, TypeScript, .NET, and Mutagen.

# What is Beyond Skyrim?

Beyond Skyrim is a modding collective dedicated to bringing other provinces of the continent of Tamriel to life using The Elder Scrolls V: Skyrim's Creation Engine and tools. This particular website has been created for the purposes of Cyrodiil, the primary location of The Elder Scrolls IV: Oblivion and its subsequent 2025 Remaster.

# Why make it?

The BS: Cyrodiil team has been struggling with keeping up with documentation of various stages of development, such as writing and editing, quality assurance, and level design. This website is meant to provide one true source of information about a particular location, thus eliminating the need of a hunt on multiple different data sources.

# How does it work?

The CI/CD pipeline launches the DataExtractor.Tool which then downloads necessary master game files. It then scrapes location data from the master files, as well as various other sources scattered on the project's cloud. It utilizes Google Sheets API, Vikunja API, as well as Mutagen to search and match location data and compile it into one true source of data.

The DataExtractor .NET API is then used to fetch and use the scraped data and display it in the UI.
