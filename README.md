# cvshortlist
__CV Shortlist__ is an AI-powered web portal designed to help professional recruiters and HR departments streamline their hiring process. The most intensive step of candidate selection is the effective shortlisting of suitable candidates, out of a large pool of applications. The value proposition of CV Shortlist is to reliably and efficiently select from among hundreds or thousands of candidate applications in an automated way.

__CV Shortlist__ relies on modern and powerful AI technology to perform the candidate CVs shortlisting:
* Microsoft Azure Document Intelligence - Extracting candidate information from PDF files, with full comprehension of complex layouts involving tables and columns
* OpenAI GPT-5 - Analyzing the extracted candidate data, matching it to the job opening description, and producing the shortlisting

This project had initially been intended as a closed-source commercial product, but was then switched into an open-source educational and self-hosted solution. It provides two web applications:
* CvShortlist - the full web application, to be hosted in Azure Cloud
* CvShortlist.SelfHosted - the self-hosted, stripped-down, web application, with only the two AI technologies as Azure Cloud dependencies

The technology stack for this project is:
* .NET 10
* ASP.NET Core 10
* Blazor 10
* Entity Framework Core 10
* Azure Cloud (Web App, SQL Database, Blob Storage, Document Intelligence, OpenAI, Communication Services, Application Insights)
* Sqlite
* HTML, CSS, Javascript (IndexedDB, Web Crypto API)
* external libraries: PdfPig, pdf-lib, zip.js, QRCoder, SkiaSharp

In order to be able to run this project, one needs an Azure Cloud account with an active subscription, and the following resources set up in Azure (these can be looked up in the code files __ConfigurationData.cs__ and __Program.cs__):
* Cloud-hosted web application - Web App, Document Intelligence, gpt-5-mini Foundry model, Storage Account, SQL Database, Application Insights, Communication Services, Email Communication Services
* self-hosted web application - Document Intelligence, gpt-5-mini Foundry model

The configuration data should be stored:
* local debugging - in __user secrets__ files
* production deployments - as __environment variables__

The license for this project is the __MIT License__, meaning it can be used in open-source and closed-source, free-of-cost and commercial, products.

![Screenshot](https://raw.githubusercontent.com/mihnea-radulescu/cvshortlist/main/Screenshot.jpg "Screenshot")
