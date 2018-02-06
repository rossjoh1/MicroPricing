FROM microsoft/aspnetcore:2.0 
WORKDIR /app
COPY bin/Release/PublishOutput .
ENV ASPNETCORE_URLS http://*:5001
EXPOSE 5001
ENTRYPOINT ["dotnet", "AppraisalPricing.dll"]
