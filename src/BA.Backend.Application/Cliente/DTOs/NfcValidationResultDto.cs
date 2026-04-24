using System;

namespace BA.Backend.Application.Cliente.DTOs;

public record NfcValidationResultDto(
    Guid NfcTagId,
    Guid CoolerId,
    Guid StoreId,
    string CoolerModel,
    int Capacity,
    int AvailableCapacity,
    DateTime? LastRevisionAt,
    string AccessToken = ""
);
