using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Organisation_WebAPI.Data;
using Organisation_WebAPI.Dtos.CustomerDto;

namespace Organisation_WebAPI.Services.Customers
{
    public class CustomerService : ICustomerService
    {   
        private readonly IMapper _mapper;  // Provides object-object mapping
        private readonly OrganizationContext _context;  // Provides object-object mapping
        public CustomerService(IMapper mapper,OrganizationContext context)
        {
            _mapper = mapper; // Injects the IMapper instance
            _context = context; // Injects the OrganizationContext instance
        }

        // Adds a new customer to the database

        public async Task<ServiceResponse<List<GetCustomerDto>>> AddCustomer(AddCustomerDto addCustomer)
        {
            var serviceResponse = new ServiceResponse<List<GetCustomerDto>>();
            try 
            {
            var customer = _mapper.Map<Customer>(addCustomer);

            var productExists = await _context.Products.AnyAsync(p => p.ProductID == addCustomer.ProductID);
            if (!productExists)
                throw new Exception($"Invalid ProductID '{addCustomer.ProductID}'");
            
            if (!IsEmailValid(addCustomer.CustomerEmail))
                throw new Exception($"Email is invalid");

             _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            serviceResponse.Data = await _context.Customers.Select(c => _mapper.Map<GetCustomerDto>(c)).ToListAsync();
            }
            catch(Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            } 
            return serviceResponse;
        }

        // Deletes a customer from the database based on the provided ID
        public async Task<ServiceResponse<List<GetCustomerDto>>> DeleteCustomer(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetCustomerDto>>();
            try {

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerID == id);
            if (customer is null)
                throw new Exception($"Customer with id '{id}' not found");
            
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            serviceResponse.Data = await _context.Customers.Select(c => _mapper.Map<GetCustomerDto>(c)).ToListAsync();
            }
            catch(Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
                return serviceResponse;
        }

        // Retrieves all customers from the database
        public async Task<ServiceResponse<List<GetCustomerDto>>> GetAllCustomers()
        {  
              var serviceResponse = new ServiceResponse<List<GetCustomerDto>>();
        var dbCustomer = await _context.Customers.ToListAsync();
        var customerDTOs = dbCustomer.Select(e => new GetCustomerDto
        {
            CustomerID = e.CustomerID,
            CustomerName = e.CustomerName,
            CustomerEmail = e.CustomerEmail,
            CustomerPhoneNumber = e.CustomerPhoneNumber,
            ProductID = e.ProductID,
            ProductName = _context.Products.FirstOrDefault(p => p.ProductID == e.ProductID)?.ProductName
        }).ToList();

        serviceResponse.Data = customerDTOs;
        return serviceResponse;
        }

        //Retrieves a customer from the database with Id
        public async Task<ServiceResponse<GetCustomerDto>> GetCustomerById(int id)
        {
            
            var serviceResponse = new ServiceResponse<GetCustomerDto>();
            try
            {
            var customer =  await _context.Customers.FirstOrDefaultAsync(c => c.CustomerID == id);
            if (customer is null)
                    throw new Exception($"Customer with id '{id}' not found");
            serviceResponse.Data = _mapper.Map<GetCustomerDto>(customer);
            return serviceResponse;
            }
            catch(Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
        return serviceResponse;
        }


        public async Task<ServiceResponse<GetCustomerDto>> UpdateCustomer(UpdateCustomerDto updatedCustomer, int id)
        {
             var serviceResponse = new ServiceResponse<GetCustomerDto>();
            try {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerID == id);
                if (customer is null)
                    throw new Exception($"Customer with id '{id}' not found");

                var productExists = await _context.Products.AnyAsync(p => p.ProductID == updatedCustomer.ProductID);
                if (!productExists)
                    throw new Exception($"Invalid ProductID '{updatedCustomer.ProductID}'");

                if (!IsEmailValid(updatedCustomer.CustomerEmail))
                    throw new Exception($"Email is invalid");
                
                customer.CustomerName = updatedCustomer.CustomerName;
                customer.CustomerEmail = updatedCustomer.CustomerEmail;
                customer.CustomerPhoneNumber = updatedCustomer.CustomerPhoneNumber;
                customer.ProductID = updatedCustomer.ProductID;

                await _context.SaveChangesAsync();
                serviceResponse.Data = _mapper.Map<GetCustomerDto>(customer);

                return serviceResponse;
            }
            catch(Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            
            return serviceResponse;
        }

        private bool IsEmailValid(string? email)
        {   
            if (email is null)
            {
                return false;
            }
            // Regular expression pattern for email validation
            string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";

            // Check if the email matches the pattern
            bool isValid = Regex.IsMatch(email, pattern);

            return isValid;
        }
    }

}