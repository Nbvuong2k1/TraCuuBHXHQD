using TraCuuBHXH_BHYT.Request;
using TraCuuBHXH_BHYT.Response;
using System.Threading.Tasks;

namespace TraCuuBHXH_BHYT.Interface
{
    public interface ITraCuuBHXHService
    {
        Task<ResponseTraCuuBHXHVN> TraCuuBHXHQDAsync(RequestTraCuuBHXHVN request);
        Task<ResponseTraCuuBHXHVN> ThemHoacCapNhatAsync(RequestTraCuuBHXHVN request);
    }
}
