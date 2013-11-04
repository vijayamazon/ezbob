using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using StructureMap;

namespace EZBob.DatabaseLib
{
    public class MarketPlacesBootstrap
    {

        private readonly MarketPlaceRepository _marketPlaceRepository;
        private readonly AnalyisisFunctionRepository _AnalyisisFunctionRepository;
        private readonly ValueTypeRepository _ValueTypeRepository;

        public MarketPlacesBootstrap(MarketPlaceRepository marketPlaceRepository, AnalyisisFunctionRepository analyisisFunctionRepository, ValueTypeRepository valueTypeRepository)
        {
            _marketPlaceRepository = marketPlaceRepository;
            _AnalyisisFunctionRepository = analyisisFunctionRepository;
            _ValueTypeRepository = valueTypeRepository;
        }

        public void InitDatabaseMarketPlaceTypes()
        {

            _marketPlaceRepository.EnsureTransaction(() =>
            {
                var mpDllTypes = ObjectFactory.GetAllInstances<IMarketplaceType>(); //all registered typed in dlls
                var allMpTypes = _marketPlaceRepository.GetAll().ToList();
                var allFunctions = _AnalyisisFunctionRepository.GetAllFunctionsAndInit();

                var mpTypesToAdd = mpDllTypes.Where(t => allMpTypes.All(dbt => dbt.InternalId != t.InternalId));
            
                foreach (var mpType in mpTypesToAdd)
                {
                    var dbMpType = AddMarketPlaceType(mpType);
                    allMpTypes.Add(dbMpType);
                }

                foreach (var mpType in allMpTypes)
                {
                    var list = mpDllTypes.Single(t => t.InternalId == mpType.InternalId).DatabaseFunctionList;

                    if (list == null) continue;

                    var fnsToAdd = list.Where(fn => allFunctions.All(dbfn => dbfn.InternalId != fn.InternalId));

                    foreach (var fn in fnsToAdd)
                    {
                        AddFunctionIfNotExists(fn, mpType, allFunctions);
                    }
                }
            });
        }

        private MP_MarketplaceType AddMarketPlaceType(IMarketplaceType databaseMarketPlace)
        {
            var marketPlace = new MP_MarketplaceType
            {
                InternalId = databaseMarketPlace.InternalId,
                Description = databaseMarketPlace.Description,
                Name = databaseMarketPlace.DisplayName
            };
            
            _marketPlaceRepository.Save(marketPlace);

            return marketPlace;
        }

        private void AddFunctionIfNotExists(IDatabaseFunction func, MP_MarketplaceType databaseMarketplace, IList<MP_AnalyisisFunction> allFunctions)
        {
            MP_ValueType mpValueType = _ValueTypeRepository.Get(func.FunctionValueType.InternalId);

            if (allFunctions.All(x => x.InternalId != func.InternalId))
            {
                var f = new MP_AnalyisisFunction
                {
                    InternalId = func.InternalId,
                    Marketplace = databaseMarketplace,
                    Name = func.Name,
                    ValueType = mpValueType,
                    Description = func.Description
                };

                allFunctions.Add(f);
                _AnalyisisFunctionRepository.Save(f);
            }
            else
            {
                var f = allFunctions.Single(fn => fn.InternalId == func.InternalId);

                if (!string.Equals(f.Name, func.Name) || f.ValueType != mpValueType || !string.Equals(f.Description, func.Description))
                {
                    f.Name = func.Name;
                    f.ValueType = mpValueType;
                    f.Description = func.Description;

                    _AnalyisisFunctionRepository.Update(f);
                }
            }
        }

        public void InitValueTypes()
        {
            _ValueTypeRepository.EnsureTransaction(() =>
            {
                var valTypes = _ValueTypeRepository.GetAll().ToList();

                foreach (DatabaseValueTypeEnum enumItem in Enum.GetValues(typeof(DatabaseValueTypeEnum)))
                {
                    var valueType = DatabaseValueTypeFactory.Create(enumItem);

                    if (valTypes.Any(v => v.InternalId == valueType.InternalId)) continue;

                    var period = new MP_ValueType
                                     {
                                         InternalId = valueType.InternalId,
                                         Name = valueType.Name,
                                         Description = valueType.Description
                                     };

                    _ValueTypeRepository.Save(period);
                }
            });
        }
    }
}