using System;
using Microsoft.Extensions.Logging;
using Models;
using Models.Interfaces;
using Models.DTO;
using Newtonsoft.Json;

namespace Services;

public class AlbumsServiceWapi : IAlbumsService
{
    private readonly ILogger<AlbumsServiceWapi> _logger;
    private readonly HttpClient _httpClient;

    //To ensure Json deserializern is using the class implementations instead of interfaces 
    private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        Converters = {
            new AbstractConverter<MusicGroup, IMusicGroup>(),
            new AbstractConverter<Album, IAlbum>(),
            new AbstractConverter<Artist, IArtist>()
        },
    };

    public AlbumsServiceWapi(IHttpClientFactory httpClientFactory, ILogger<AlbumsServiceWapi> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(name: "MusicWebApi");
    }

    public async Task<ResponsePageDto<IAlbum>> ReadAlbumsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        string uri = $"albums/read?seeded={seeded}&flat={flat}&filter={filter}&pagenr={pageNumber}&pagesize={pageSize}";

        //Send the HTTP Message and await the repsonse
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        //Throw an exception if the response is not successful
        await response.EnsureSuccessStatusMessage();

        //Get the resonse data
        string s = await response.Content.ReadAsStringAsync();
        var resp = JsonConvert.DeserializeObject<ResponsePageDto<IAlbum>>(s, _jsonSettings);
        return resp;
    }
    public async Task<ResponseItemDto<IAlbum>> ReadAlbumAsync(Guid id, bool flat)
    {
        string uri = $"albums/readitem?id={id}&flat={flat}";

        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var resp = JsonConvert.DeserializeObject<ResponseItemDto<IAlbum>>(s, _jsonSettings);
        return resp;
    }
    public async Task<ResponseItemDto<IAlbum>> DeleteAlbumAsync(Guid id)
    {
        string uri = $"albums/deleteitem/{id}";

        HttpResponseMessage response = await _httpClient.DeleteAsync(uri);

        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var resp = JsonConvert.DeserializeObject<ResponseItemDto<IAlbum>>(s, _jsonSettings);
        return resp;
    }
    public async Task<ResponseItemDto<IAlbum>> UpdateAlbumAsync(AlbumCUdto item)
    {
        string uri = $"albums/updateitem/{item.AlbumId}";
        string json = JsonConvert.SerializeObject(item);

        HttpResponseMessage response = await _httpClient.PutAsync(uri, new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var resp = JsonConvert.DeserializeObject<ResponseItemDto<IAlbum>>(s, _jsonSettings);
        return resp;
    }
    public async Task<ResponseItemDto<IAlbum>> CreateAlbumAsync(AlbumCUdto item)
    {
        
        // If statement needed to avoid zero CopiesSold throwing an exception in EnsureValidity 
        if (item.CopiesSold == 0)
        {
            item.CopiesSold = Random.Shared.Next(1_000, 1_000_000);
        }

        string uri = $"albums/createitem";
        string json = JsonConvert.SerializeObject(item);

        HttpResponseMessage response = await _httpClient.PostAsync(uri, new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var resp = JsonConvert.DeserializeObject<ResponseItemDto<IAlbum>>(s, _jsonSettings);
        return resp;
    }
}

