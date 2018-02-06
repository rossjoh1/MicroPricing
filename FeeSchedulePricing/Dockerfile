FROM microsoft/aspnetcore:2.0 
WORKDIR /app
COPY bin/Release/PublishOutput .
ENV ASPNETCORE_URLS http://*:5002
EXPOSE 5002
ENTRYPOINT ["dotnet", "FeeSchedulePricing.dll"]
