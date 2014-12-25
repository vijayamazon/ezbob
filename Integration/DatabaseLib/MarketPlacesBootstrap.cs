namespace EZBob.DatabaseLib {
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using StructureMap;

	public class MarketPlacesBootstrap {
		public MarketPlacesBootstrap(MarketPlaceRepository marketPlaceRepository, ValueTypeRepository valueTypeRepository) {
			_marketPlaceRepository = marketPlaceRepository;
			_ValueTypeRepository = valueTypeRepository;
		}

		public void InitDatabaseMarketPlaceTypes() {
			_marketPlaceRepository.EnsureTransaction(() => {
				var mpDllTypes = ObjectFactory.GetAllInstances<IMarketplaceType>(); //all registered typed in dlls
				var allMpTypes = _marketPlaceRepository.GetAll().ToList();

				var mpTypesToAdd = mpDllTypes.Where(t => allMpTypes.All(dbt => dbt.InternalId != t.InternalId));

				foreach (var mpType in mpTypesToAdd) {
					var dbMpType = AddMarketPlaceType(mpType);
					allMpTypes.Add(dbMpType);
				}
			});
		}

		public void InitValueTypes() {
			_ValueTypeRepository.EnsureTransaction(() => {
				var valTypes = _ValueTypeRepository.GetAll().ToList();

				foreach (DatabaseValueTypeEnum enumItem in Enum.GetValues(typeof(DatabaseValueTypeEnum))) {
					var valueType = DatabaseValueTypeFactory.Create(enumItem);

					if (valTypes.Any(v => v.InternalId == valueType.InternalId))
						continue;

					var period = new MP_ValueType {
						InternalId = valueType.InternalId,
						Name = valueType.Name,
						Description = valueType.Description
					};

					_ValueTypeRepository.Save(period);
				}
			});
		}

		private MP_MarketplaceType AddMarketPlaceType(IMarketplaceType databaseMarketPlace) {
			var marketPlace = new MP_MarketplaceType {
				InternalId = databaseMarketPlace.InternalId,
				Description = databaseMarketPlace.Description,
				Name = databaseMarketPlace.DisplayName
			};

			_marketPlaceRepository.Save(marketPlace);

			return marketPlace;
		}

		private readonly MarketPlaceRepository _marketPlaceRepository;
		private readonly ValueTypeRepository _ValueTypeRepository;
	}
}