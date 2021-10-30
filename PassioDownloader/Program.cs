using PassioAPI;

PassioAPI.PassioAPI api = new PassioAPI.PassioAPI("http://passio3.com", 3000);
await api.GetStops();