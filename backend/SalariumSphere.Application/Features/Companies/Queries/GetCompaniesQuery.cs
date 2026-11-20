using MediatR;
using SalariumSphere.Application.Features.Companies.Common;

namespace SalariumSphere.Application.Features.Companies.Queries;

public class GetCompaniesQuery : IRequest<List<CompanyDto>> { }