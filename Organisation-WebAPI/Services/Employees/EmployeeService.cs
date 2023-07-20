using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EmailService;
using Microsoft.EntityFrameworkCore;
using Organisation_WebAPI.Data;
using Organisation_WebAPI.Dtos.EmployeeDto;
using Organisation_WebAPI.Dtos.ManagerDto;

namespace Organisation_WebAPI.Services.Employees
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IMapper _mapper;  // Provides object-object mapping
        private readonly OrganizationContext _context ; // Represents the database context
        private readonly IEmailSender _emailSender;

        public EmployeeService(IMapper mapper,OrganizationContext context)
        {
            _context = context; // Injects the OrganizationContext instance
            _mapper = mapper; // Injects the IMapper instance
            
        }

        // Adds a new Employee to the database
        public async Task<ServiceResponse<List<GetEmployeeDto>>> AddEmployee(AddEmployeeDto newEmployee)
        {
            var serviceResponse = new ServiceResponse<List<GetEmployeeDto>>();
            var employee = _mapper.Map<Employee>(newEmployee);

             _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            serviceResponse.Data = await _context.Employees.Select(c => _mapper.Map<GetEmployeeDto>(c)).ToListAsync();
            return serviceResponse;
        }

        // Deletes a employee from the database based on the provided ID
        public async Task<ServiceResponse<List<GetEmployeeDto>>> DeleteEmployee(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetEmployeeDto>>();
            try {

            var employee = await _context.Employees.FirstOrDefaultAsync(c => c.EmployeeID == id);
            if (employee is null)
                throw new Exception($"Employee with id '{id}' not found");
            
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            serviceResponse.Data = _context.Employees.Select(c => _mapper.Map<GetEmployeeDto>(c)).ToList();
            }
            catch(Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
             return serviceResponse;
        }

        // Retrieves all employees from the database
       public async Task<ServiceResponse<List<GetEmployeeDto>>> GetAllEmployees()
       {
        var serviceResponse = new ServiceResponse<List<GetEmployeeDto>>();

        try {
        var dbEmployees = await _context.Employees.ToListAsync();
        var employeeDTOs = dbEmployees.Select(e =>
        {
            var employeeDto = _mapper.Map<GetEmployeeDto>(e);
            var manager = _context.Managers
                .Include(m => m.Department)
                .FirstOrDefault(m => m.ManagerId == e.ManagerID);
            employeeDto.DepartmentName = manager?.Department?.DepartmentName;
            employeeDto.ManagerName = manager?.ManagerName;
            employeeDto.ManagerIsAppointed = manager?.IsAppointed;
            return employeeDto;
        }).ToList();


                serviceResponse.Data = employeeDTOs;
        } 

        catch(Exception ex)
         {
                serviceResponse.Success = false;
                serviceResponse.Message = "Error occured while fetching Employee Details " + ex;
        }
        return serviceResponse;
        }





        //Retrieves a employee from the database with Id
         public async Task<ServiceResponse<GetEmployeeDto>> GetEmployeeById(int id)
        {
            
            var serviceResponse = new ServiceResponse<GetEmployeeDto>();
            try
            {
                var dbEmployee = await _context.Employees
                   .Include(e => e.Manager)
                   .FirstOrDefaultAsync(c => c.EmployeeID == id);

                if (dbEmployee is null)
                    throw new Exception($"Employee with id '{id}' not found");

                var manager = dbEmployee.Manager;
                if (manager is null)
                    throw new Exception($"Manager not found for Employee with id '{id}'");

                var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentID == manager.DepartmentID);

                var getEmployeeDto = _mapper.Map<GetEmployeeDto>(dbEmployee);
                getEmployeeDto.DepartmentName = department?.DepartmentName;

                serviceResponse.Data = getEmployeeDto;
            }
            catch(Exception ex)
            {   
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetEmployeeDto>>> GetAllEmployeesByManagerId(int managerId)
        {
            var serviceResponse = new ServiceResponse<List<GetEmployeeDto>>();
            try
            {
                var manager = await _context.Managers
                    .Include(m => m.Employees)
                    .FirstOrDefaultAsync(m => m.ManagerId == managerId);

                if (manager == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Manager not found.";
                    return serviceResponse;
                }

                var employees = manager.Employees.ToList();
                var employeeDtos = employees.Select(e =>
                {
                    var employeeDto = _mapper.Map<GetEmployeeDto>(e);
                    return employeeDto;
                }).ToList();

                serviceResponse.Data = employeeDtos;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }



        public async Task<ServiceResponse<UpdateEmployeeDto>> UpdateEmployee(UpdateEmployeeDto updatedEmployee, int id)
        {
            var serviceResponse = new ServiceResponse<UpdateEmployeeDto>();
            try {
                var employee = await _context.Employees.FirstOrDefaultAsync(c => c.EmployeeID == id);

                if (employee is null)
                    throw new Exception($"Employee with id '{id}' not found");

                employee.EmployeeName = updatedEmployee.EmployeeName;
                employee.EmployeeSalary = updatedEmployee.EmployeeSalary;
                employee.EmployeeAge = updatedEmployee.EmployeeAge;
                employee.ManagerID = updatedEmployee.ManagerID;
                employee.Phone = updatedEmployee.Phone;
                employee.Address = updatedEmployee.Address;
                await _context.SaveChangesAsync();
                serviceResponse.Data = _mapper.Map<UpdateEmployeeDto>(employee);

                return serviceResponse;
            }
            catch(Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            
            return serviceResponse;
        
        }


        //public async Task<ServiceResponse<List<GetEmployeeDto>>> GetAllEmployeesByProduct(int productId)
        //{
        //    var serviceResponse = new ServiceResponse<List<GetEmployeeDto>>();
        //    try
        //    {
        //        var manager = await _context.Managers
        //            .Include(m => m.Employees)
        //            .Include(m => m.Product)
        //            .FirstOrDefaultAsync(m => m.ProductID == productId);

        //        if (manager == null)
        //        {
        //            serviceResponse.Success = false;
        //            serviceResponse.Message = "No manager found for the product.";
        //            return serviceResponse;
        //        }

        //        var employees = manager.Employees.ToList();
        //        var employeeDtos = employees.Select(e => _mapper.Map<GetEmployeeDto>(e)).ToList();

        //        // Add product name to each employee DTO
        //        employeeDtos.ForEach(e =>
        //        {
        //            e.ProductName = manager.Product.ProductName;
        //        });

        //        serviceResponse.Data = employeeDtos;
        //    }
        //    catch (Exception ex)
        //    {
        //        serviceResponse.Success = false;
        //        serviceResponse.Message = ex.Message;
        //    }
        //    return serviceResponse;
        //}




    }
}