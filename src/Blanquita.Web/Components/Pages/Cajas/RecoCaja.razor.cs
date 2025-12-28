using Blanquita.Application.DTOs;
using Blanquita.Application.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Blanquita.Web.Components.Pages.Cajas;

public partial class RecoCaja
{
    [Inject] public IPrintingService PrintingService { get; set; } = default!;
    [Inject] public ICashRegisterService CashRegisterService { get; set; } = default!;
    [Inject] public ICashierService CashierService { get; set; } = default!;
    [Inject] public ISupervisorService SupervisorService { get; set; } = default!;
    [Inject] public ICashCollectionService CashCollectionService { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;

    private int _selectedSupervisorId = 0;
    private List<SupervisorDto>? supervisors;
    private int _selectedCashierId = 0;
    private List<CashierDto>? cashiers;
    private int _selectedCashRegisterId = 0;
    private List<CashRegisterDto>? cashRegisters;

    // Denominations
    private int _c1000 = 0;
    private int _c500 = 0;
    private int _c200 = 0;
    private int _c100 = 0;
    private int _c50 = 0;
    private int _c20 = 0;

    private int _total = 0;
    private bool _isError = false;
    private bool _isSubmitting = false;
    private string _errorMessage = string.Empty;

    private SupervisorDto? _selectedSupervisor;
    private CashierDto? _selectedCashier;
    private CashRegisterDto? _selectedCashRegister;

    protected override async Task OnInitializedAsync()
    {
        supervisors = (await SupervisorService.GetAllAsync()).ToList();
        cashiers = new List<CashierDto>();
        cashRegisters = new List<CashRegisterDto>();
    }

    private async Task OnSupervisorChanged(int supervisorId)
    {
        _selectedSupervisorId = supervisorId;
        _selectedCashierId = 0;
        _selectedCashRegisterId = 0;

        if (supervisorId > 0)
        {
            _selectedSupervisor = await SupervisorService.GetByIdAsync(supervisorId);
            if (_selectedSupervisor != null)
            {
                cashRegisters = (await CashRegisterService.GetByBranchAsync(_selectedSupervisor.BranchId)).ToList();
                cashiers = (await CashierService.GetByBranchAsync(_selectedSupervisor.BranchId)).ToList();
            }
        }
        else
        {
            cashRegisters = new List<CashRegisterDto>();
            cashiers = new List<CashierDto>();
        }

        StateHasChanged();
    }

    private bool ValidateForm()
    {
        if (_selectedSupervisorId == 0)
        {
            Snackbar.Add("❗Seleccione una encargada", Severity.Warning);
            return false;
        }

        if (_selectedCashierId == 0)
        {
            _errorMessage = "Seleccione una cajera";
            return false;
        }

        if (_selectedCashRegisterId == 0)
        {
            _errorMessage = "Seleccione una caja";
            return false;
        }

        _total = (_c1000 * 1000) + (_c500 * 500) + (_c200 * 200) + (_c100 * 100) + (_c50 * 50) + (_c20 * 20);

        if (_total <= 0)
        {
            _errorMessage = "El monto total debe ser mayor a cero";
            return false;
        }

        _errorMessage = string.Empty;
        return true;
    }

    private async Task ImprimirReco()
    {
        if (_isSubmitting) return;

        _isSubmitting = true;
        _errorMessage = string.Empty;
        try
        {
            if (!ValidateForm())
            {
                _isError = true;
                StateHasChanged();
                return;
            }

            _selectedCashRegister = await CashRegisterService.GetByIdAsync(_selectedCashRegisterId);
            _selectedCashier = await CashierService.GetByIdAsync(_selectedCashierId);
            _selectedSupervisor = await SupervisorService.GetByIdAsync(_selectedSupervisorId);

            if (_selectedCashRegister == null || _selectedCashier == null || _selectedSupervisor == null)
            {
                _errorMessage = "Error al cargar datos seleccionados.";
                _isError = true;
                return;
            }

            var createDto = new CreateCashCollectionDto
            {
                Thousands = _c1000,
                FiveHundreds = _c500,
                TwoHundreds = _c200,
                Hundreds = _c100,
                Fifties = _c50,
                Twenties = _c20,
                CashRegisterName = _selectedCashRegister.Name,
                CashierName = _selectedCashier.Name,
                SupervisorName = _selectedSupervisor.Name,
                IsForCashCut = false
            };

            // Guardar la recolección
            var recoleccionGuardada = await CashCollectionService.CreateAsync(createDto);

            try
            {
                await PrintingService.PrintCashCollectionAsync(recoleccionGuardada, _selectedCashRegister.PrinterIp, _selectedCashRegister.PrinterPort);
                Snackbar.Add("✅ Recoleccion impresa correctamente", Severity.Success);
            }
            catch
            {
                _isError = true;
                _errorMessage = $"Error al conectar con impresora reintentando imprimir en impresora: {_selectedCashRegister.Name}...";
                Snackbar.Add(_errorMessage, Severity.Warning);

                // Use the new service method for backup logic
                try
                {
                    await Task.Delay(5000); // 5 seconds delay
                    var respaldo = await CashRegisterService.GetBackupRegisterAsync(_selectedCashRegisterId);
                    
                    if (respaldo != null)
                    {
                        await PrintingService.PrintCashCollectionAsync(recoleccionGuardada, respaldo.PrinterIp, respaldo.PrinterPort);
                        Snackbar.Add($"⚠️ Impresión con respaldo: {respaldo.Name}", Severity.Warning);
                        _isError = false; // Recovered
                        _errorMessage = string.Empty;
                    }
                    else
                    {
                        Snackbar.Add("❌ No se encontró impresora de respaldo.", Severity.Error);
                    }
                }
                catch (Exception e2)
                {
                    _errorMessage = $"Error al Imprimir contacte a sistemas: {e2.Message}";
                    _isError = true;
                }
            }

            ResetForm();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error al procesar la recolección: {ex.Message}";
            _isError = true;
        }
        finally
        {
            _isSubmitting = false;
            StateHasChanged();
        }
    }

    private void ResetForm()
    {
        _selectedSupervisorId = 0;
        _selectedCashierId = 0;
        _selectedCashRegisterId = 0;
        _c1000 = 0;
        _c500 = 0;
        _c200 = 0;
        _c100 = 0;
        _c50 = 0;
        _c20 = 0;
        _total = 0;
    }
}
