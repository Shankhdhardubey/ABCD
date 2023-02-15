using System;
using DataModelMigration.Common;
using DataModelMigration.Service;
using System.Collections.Generic;
using System.Text;
using DataModelMigration.DAL;
using DataModelMigration.Model.DataModel;
using System.Linq;

namespace DataModelMigration.Script
{
    class DataModelMigrationcs
    {
        public string Update_Description_Risk { get; set; } = $@"----------/**/----------
			UPDATE Risk
SET description = (
		SELECT description
		FROM GIS_Data_Model
		WHERE code = '{AppConfiguration.TargetDataModelCode}'
		)
WHERE risk_cnt IN (
		SELECT IFRL.risk_cnt
		FROM gis_policy_link GPL
		INNER JOIN insurance_file_risk_link IFRL ON GPL.insurance_file_cnt = IFRL.insurance_file_cnt
		INNER JOIN risk R ON R.risk_cnt = IFRL.risk_cnt
		WHERE GPL.gis_data_model_id = (
				SELECT gis_data_model_id
				FROM [GIS_Data_Model]
				WHERE code = '{AppConfiguration.SourceDataModelCode}'
				)
		)";

        public string Update_risk_code_id_Insurance_File { get; set; } = $@"----------/**/----------
UPDATE insurance_file
SET risk_code_id = (
		SELECT CASE 
				WHEN isnull(HRD.Cover_type, 1) = 1
					THEN (
							SELECT risk_code_id
							FROM Risk_code
							WHERE code = 'GuestHse'
							)
				WHEN HRD.Cover_type = 2
					THEN (
							SELECT risk_code_id
							FROM Risk_code
							WHERE code = 'HOLHOM'
							)
				WHEN HRD.Cover_type = 3
					THEN (
							SELECT risk_code_id
							FROM Risk_code
							WHERE code = 'STAFFACC'
							)
				END AS covertype
		)
FROM insurance_file i
INNER JOIN gis_policy_link GPL ON i.insurance_file_cnt = GPL.insurance_file_cnt
INNER JOIN [HIUAGH_Policy_Binder] HPB ON HPB.gis_policy_link_id = GPL.gis_policy_link_id
INNER JOIN [HIUAGH_RISK_DETAILS] HRD ON HPB.HIUAGH_Policy_binder_id = HRD.HIUAGH_Policy_binder_id
WHERE GPL.gis_data_model_id = (
		SELECT gis_data_model_id
		FROM [GIS_Data_Model]
		WHERE code = '{AppConfiguration.SourceDataModelCode}'
		)
--Risk_code_id update 'guesthse' for those whose record are not in Gis_policy_link
UPDATE i
	    SET i.risk_code_id = (SELECT risk_code_id FROM risk_code WHERE code = 'GuestHse')
	    FROM insurance_file i
		JOIN Risk_code as rc on rc.risk_code_id = i.risk_code_id
		WHERE rc.code = '{AppConfiguration.SourceDataModelCode}'
";

        public string Update_Gis_Data_Model_Id_Gis_Data_Model { get; set; } = $@"----------/**/----------

        UPDATE GPL
SET GPL.gis_data_model_id = (
        SELECT gis_data_model_id

        FROM GIS_Data_Model
        WHERE code = '{AppConfiguration.TargetDataModelCode}'
		)
FROM gis_policy_link GPL
INNER JOIN GIS_Data_Model GDM ON GPL.gis_data_model_id = GDM.gis_data_model_id
WHERE GDM.code = '{AppConfiguration.SourceDataModelCode}'";

    

        public string Policy_binder { get; set; } = $@"----------/**/----------
            insert into {AppConfiguration.TargetDataModelCode}_Policy_binder 
             select* from {AppConfiguration.SourceDataModelCode}_Policy_Binder";

        //        public string Policy { get; set; } = $@"----------/**/----------
        //insert into {AppConfiguration.TargetDataModelCode}_policy(
        public string DataModelCode_Policy { get; set; }= $@"----------/**/----------
            insert into {AppConfiguration.TargetDataModelCode}_Policy 
             select* from {AppConfiguration.SourceDataModelCode}_Policy";

        #region Guest house data migration

        /// <summary>
        /// Migrate new data models which have data in the source general details table for Guest House
        /// </summary>
        public string General_Details { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_GENERAL_DETAILS
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_GENERAL_DETAILS_id, Insured, Trading_Name, Contact_Name, 
            Details_company_status, Member_of_Cumbria, address_cnt, Email_address, Website_address, ERN,Emp_Ref_no,
            Turnover, Target_premium, Third_Party_Interest, Company_status, Previous_insurer)
                select  HGD.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                        HGD.{AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS_id, HGD.Insured, HGD.Trading_Name, 
                        HGD.Contact_Name, HGD.details_company_status, HGD.Member_of_Cumbria, HGD.address_cnt, 
                        NULL as Email_Address, HGD.Website_address, HGD.ERN, HGD.Emp_Ref_no, HBC.Turnover, 
                        HGD.Target_Premium, HGD.Third_party_interest, 
		                case when exists (select UDL_GH_CS_id from UDL_GH_CS where UDL_GH_CS_id=HGD.Company_status)
			             then 
			                (select c.UDL_IUA_COMPANY_STATUS_id from Generic_Lookup_Data a 
                                join UDL_GH_CS b on a.Source_Code = b.code
			                    join UDL_IUA_COMPANY_STATUS c on a.Target_Code = c.code
			                 where LookUp_Name='LUGENERALDETAILSCompanyStatus' and UDL_GH_CS_id=HGD.Company_status)
			             else
			                NULL
			            end as Company_status, 
                        (select UDL_IUA_PREVIOUS_INSURER_id from UDL_IUA_PREVIOUS_INSURER join pmcaption 
					        on pmcaption.caption_id = UDL_IUA_PREVIOUS_INSURER.caption_id 
					        and caption ='Unknown') as Current_insurer
                from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                    join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS HGD 
                        on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=HGD.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                    join {AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION HBC
                        on HGD.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=HBC.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                where NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_GENERAL_DETAILS 
                                    where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
					                    and HGD.{AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS_id = {AppConfiguration.TargetDataModelCode}_GENERAL_DETAILS_id)";

        /// <summary>
        /// Migrate new data models which have data in the source interested party table for Guest House
        /// </summary>
        public string Interested_Party { get; set; } = $@"----------/**/----------
                select HGD.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                        HGD.{AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS_id, 
		            (select ISNULL(max({AppConfiguration.TargetDataModelCode}_INTERESTED_PARTY_id),0)  
                        from {AppConfiguration.TargetDataModelCode}_INTERESTED_PARTY)
                        + ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) as INTERESTED_PARTY_id, 
                    NULL as IP_Name, NULL as address_cnt, HGD.TP_Details 
		        into #tempT
		        from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
			        join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS HGD 
                        on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=HGD.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
		        where NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_INTERESTED_PARTY 
                                    where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
					                    and HGD.{AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS_id = {AppConfiguration.TargetDataModelCode}_GENERAL_DETAILS_id)

                update #tempT set INTERESTED_PARTY_id = (select ISNULL(max(INTERESTED_PARTY_id),0) from #tempT) + 1
                where {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS_id = INTERESTED_PARTY_id

                INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_INTERESTED_PARTY
                    ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
                    {AppConfiguration.TargetDataModelCode}_GENERAL_DETAILS_id, 
                    {AppConfiguration.TargetDataModelCode}_INTERESTED_PARTY_id,
                    IP_Name, address_cnt, TP_Details)
                select {AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                        {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS_id, INTERESTED_PARTY_id, 
                        IP_Name, address_cnt, TP_Details 
                from #tempT order by {AppConfiguration.SourceDataModelCode}_Policy_binder_id

		                    --for a given binder id other two ids cann't be same example row 2 of above select result set 

                drop table #tempT";


        /// <summary>
        /// Migrate new data models which have data in the source Claims table for Guest House
        /// </summary>
        public string Claim_Information { get; set; } = $@"----------/**/----------
                INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_CLAIM_INFORMATION
                   ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
                    {AppConfiguration.TargetDataModelCode}_CLAIM_INFORMATION_id, Had_claims)
                select  hc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                        hc.{AppConfiguration.SourceDataModelCode}_CLAIMS_id, 
		                case when ((hc.Had_liability_claim is NULL) and (hc.Had_other_clm is NULL)) then NULL
		                     when ((hc.Had_liability_claim = 0) and (hc.Had_other_clm = 0)) then 0
			                 else 1
		                end as had_claims  
                from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                    join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS HGD 
                        on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=HGD.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                    join {AppConfiguration.SourceDataModelCode}_CLAIMS hc 
                        on hc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=HGD.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                where NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_CLAIM_INFORMATION 
                                    where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
					                    and hc.{AppConfiguration.SourceDataModelCode}_CLAIMS_id = {AppConfiguration.TargetDataModelCode}_CLAIM_INFORMATION_id)";

        /// <summary>
        /// Migrate new data models which have data in the source LIABLITY and Other Claims table for Guest House
        /// </summary>
        public string Claim_Details { get; set; } = $@"----------/**/----------
               INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_CLAIM_DETAILS
                        ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
                        {AppConfiguration.TargetDataModelCode}_CLAIM_INFORMATION_id, 
                        {AppConfiguration.TargetDataModelCode}_CLAIM_DETAILS_id,
                        The_month, The_year, Paid_amount, Reserve_amount, Total_amount, Claim_description,
                        Claim_relation, Claim_details, Claim_peril, Claim_status, Claim_section)
	           select   {AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                        {AppConfiguration.SourceDataModelCode}_CLAIMS_id, ClaimId, 
			            The_month, The_year, Paid, Reserve, Total, Claim_description, 
                        Claim_relation, Claim_details,	Liability_peril, Claim_status, Claim_Section 
               from  
	                (
                      select hlc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                        hlc.{AppConfiguration.SourceDataModelCode}_CLAIMS_id, 
                        hlc.{AppConfiguration.SourceDataModelCode}_LIABLITY_CLAIMS_id as ClaimId,
                        'LIABLITY' as ClaimType, hlc.The_month, hlc.The_year,
		                case when exists (select UDL_GH_STATUS_id from UDL_GH_STATUS 
                                            where UDL_GH_STATUS_id=hlc.Claim_status)
			                 then 
			                     (select c.UDL_IUA_CLM_STATUS_id 
                                    from Generic_Lookup_Data a 
                                    join UDL_GH_STATUS b on a.Source_Code = b.code
			                        join UDL_IUA_CLM_STATUS c on a.Target_Code = c.code
			                     where LookUp_Name='LUClaimDetailsClaimStatus' and UDL_GH_STATUS_id=hlc.Claim_status)
			                 else
			                    NULL
			                 end as Claim_Status,
                        hlc.Paid, hlc.Reserve, hlc.Total, hlc.Liability_peril, hlc.Claim_description, 
		                NULL as Claim_Section, NULL as Claim_relation, NULL as Claim_details
		            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
			            join {AppConfiguration.SourceDataModelCode}_LIABLITY_CLAIMS hlc 
                            on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id =hlc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id 
			            join {AppConfiguration.SourceDataModelCode}_CLAIMS hc 
                            on hlc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id =hc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id 
			                     and hlc.{AppConfiguration.SourceDataModelCode}_CLAIMS_id=hc.{AppConfiguration.SourceDataModelCode}_CLAIMS_id
		            where hlc.The_month IS NOT NULL and hlc.The_year IS NOT NULL and hlc.Claim_status IS NOT NULL 
                        and hlc.Total IS NOT NULL
		                AND NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_CLAIM_DETAILS 
                                            where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
					                            and hlc.{AppConfiguration.SourceDataModelCode}_CLAIMS_id = {AppConfiguration.TargetDataModelCode}_CLAIM_INFORMATION_id
					                            and hlc.{AppConfiguration.SourceDataModelCode}_LIABLITY_CLAIMS_id = {AppConfiguration.TargetDataModelCode}_CLAIM_DETAILS_id)
		            union All
		            select hoc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                        hoc.{AppConfiguration.SourceDataModelCode}_CLAIMS_id, 
                        hoc.{AppConfiguration.SourceDataModelCode}_OTHER_CLAIMS_id as ClaimId,
                        'OTHER' as ClaimType, hoc.The_month, hoc.The_year,
		                case when exists (select UDL_GH_STATUS_id from UDL_GH_STATUS 
                                                where UDL_GH_STATUS_id=hoc.Claim_status)
			                 then 
			                     (select c.UDL_IUA_CLM_STATUS_id 
                                    from Generic_Lookup_Data a 
                                        join UDL_GH_STATUS b on a.Source_Code = b.code
			                            join UDL_IUA_CLM_STATUS c on a.Target_Code = c.code
			                        where LookUp_Name='LUClaimDetailsClaimStatus' 
                                        and UDL_GH_STATUS_id=hoc.Claim_status)
			                 else
			                      NULL
			                 end as Claim_Status,
                        NULL as Paid, hoc.Reserve, hoc.Total, NULL as Liability_peril, hoc.Claim_description, 
		                NULL as Claim_Section, NULL as Claim_relation, NULL as Claim_details
		            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
			            join {AppConfiguration.SourceDataModelCode}_OTHER_CLAIMS hoc 
                            on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id =hoc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id 
			            join {AppConfiguration.SourceDataModelCode}_CLAIMS c 
                            on hoc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id =c.{AppConfiguration.SourceDataModelCode}_Policy_binder_id 
			                    and hoc.{AppConfiguration.SourceDataModelCode}_CLAIMS_id=c.{AppConfiguration.SourceDataModelCode}_CLAIMS_id
		            where hoc.The_month IS NOT NULL and hoc.The_year IS NOT NULL and hoc.Claim_status IS NOT NULL 
                        and hoc.Total IS NOT NULL
		                AND NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_CLAIM_DETAILS 
                                            where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
					                            and hoc.{AppConfiguration.SourceDataModelCode}_CLAIMS_id = {AppConfiguration.TargetDataModelCode}_CLAIM_INFORMATION_id
					                            and hoc.{AppConfiguration.SourceDataModelCode}_OTHER_CLAIMS_id = {AppConfiguration.TargetDataModelCode}_CLAIM_DETAILS_id)
                    ) fnl";

        /// <summary>
        /// Migrate new data models which have data in the source declarations table for Guest House
        /// </summary>
        public string Declarations { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_DECLARATIONS
                    ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
                    {AppConfiguration.TargetDataModelCode}_DECLARATIONS_id, Prosecuted, Details_Prosecuted,
                    Refused_Insurance, Details_Refused, Bankrupt, Details_Bankrupt, Owner, Details_Owner,
                    CCJ, Details_CCJ, Company_Director, Details_Company_Director)
            select  hd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                    hd.{AppConfiguration.SourceDataModelCode}_DECLARATIONS_id, hd.Prosecuted, hd.Details,
		            hd.Refused_Insurance, hd.Details, hd.Bankrupt, hd.Details, hd.Owner, hd.Details, 
                    hd.CCJ, hd.Details, hd.Company_Director, hd.Details
            from {AppConfiguration.SourceDataModelCode}_DECLARATIONS hd 
                join {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                    on hd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
            where  NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_DECLARATIONS 
                                    where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
					                    and hd.{AppConfiguration.SourceDataModelCode}_DECLARATIONS_id = {AppConfiguration.TargetDataModelCode}_DECLARATIONS_id)";

        /// <summary>
        /// Migrate new data models which have data in the source policy cover table for Guest House
        /// </summary>
        public string Policy_Cover { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_POLICY_COVER
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_POLICY_COVER_id, Buildings, Contents, 
            Legal, Terrorism, Risk_Management, address_cnt, At_this_premises,
            Elsewhere, Listed, HH_Use, HH_Let_property, Mapview_information)
            select hrd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
            hrd.{AppConfiguration.SourceDataModelCode}_RISK_DETAILS_id, hrd.Buildings, hrd.Contents,
		    hrd.Legal, hrd.Terrorism, NULL as Risk_Management, hrd.address_cnt, hgd.At_this_premises, 
            hgd.Elsewhere, hbc.Listed, hbc.HH_Use, NULL as HH_Let_property, NULL as Mapview_information
            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                join {AppConfiguration.SourceDataModelCode}_RISK_DETAILS hrd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hrd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS hgd 
                    on hrd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hgd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION hbc 
                    on hrd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hbc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
            where  NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_POLICY_COVER 
                                    where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
					                    and hrd.{AppConfiguration.SourceDataModelCode}_RISK_DETAILS_id = {AppConfiguration.TargetDataModelCode}_POLICY_COVER_id)";

        /// <summary>
        /// Migrate new data models which have data in the source Building Construction table for Guest House
        /// </summary>
        public string Building_Construction { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_BUILDING_CONSTRUCTION
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_BUILDING_CONSTRUCTION_id, Year_of_construction,
            Listed, Thatched, Standard_construction, Flat_roof, Flat_roof_perc, Age_of_flat_roof, Flat_roof_details,
            FR_Exposed_timber, FR_Exposed_timber_perc, Grade_of_listing, State_of_repair, Flat_roof_material)
            select	hbc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                hbc.{AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION_id, 
                hbc.Age_of_building as Year_of_construction, hbc.Listed, hbc.Thatched, 
			    case when (hbc.Are_all_roofs = 1 and hbc.Are_all_the_walls = 1) 
				        then 1
				    else 0	end as Standard_construction, 
                hbc.Flat_area, hbc.Roof_per, hbc.Age_of_flat_roof, hbc.Material_details, NULL as FR_Exposed_timber, 
                NULL as FR_Exposed_timber_perc,
                case when exists (select UDL_GH_Listed_id from UDL_GH_Listed 
                                        where UDL_GH_Listed_id=hbc.Grade_of_listing)
		             then 
		                (select c.UDL_IUA_GRADE_LISTING_id from Generic_Lookup_Data a 
                            join UDL_GH_LISTED b on a.Source_Code = b.code
		                    join UDL_IUA_GRADE_LISTING c on a.Target_Code = c.code
		                 where LookUp_Name='LUPOLICYCOVERListed1' and UDL_GH_Listed_id=hbc.Grade_of_listing)
	                when exists (select UDL_GH_Listed1_id from UDL_GH_Listed1 
                                    where UDL_GH_Listed1_id=hbc.Grade_of_listing1)
		              then 
		                 (select c.UDL_IUA_GRADE_LISTING_id from Generic_Lookup_Data a 
                            join UDL_GH_LISTED1 b on a.Source_Code = b.code
		                    join UDL_IUA_GRADE_LISTING c on a.Target_Code = c.code
		                 where LookUp_Name='LUPOLICYCOVERListed2' and UDL_GH_Listed1_id=hbc.Grade_of_listing1)
	                else
		                hbc.Grade_of_listing
                end as Grade_of_listing, hbc.Good_state_of_repair as State_of_Repair, 
                case when exists (select UDL_GH_Material_id from UDL_GH_Material 
                                    where UDL_GH_Material_id=hbc.Roof_material)
		             then 
		                 (select c.UDL_IUA_ROOF_MATERIAL_id from Generic_Lookup_Data a 
                            join UDL_GH_Material b on a.Source_Code = b.code
		                    join UDL_IUA_ROOF_MATERIAL c on a.Target_Code = c.code
		                 where LookUp_Name='LUFlatRoofMaterial' and UDL_IUA_ROOF_MATERIAL_id=hbc.Roof_material)
	            else
		            hbc.Roof_material
             end as Flat_roof_material
            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS hgd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hgd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION hbc 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hbc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
            where  NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_BUILDING_CONSTRUCTION where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
					and hbc.{AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION_id = {AppConfiguration.TargetDataModelCode}_BUILDING_CONSTRUCTION_id)";


        /// <summary>
        /// Migrate new data models which have data in the source Building Construction table to wall construction table in source table for Guest House
        /// </summary>
        public string Wall_Construction { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_WALL_CONSTRUCTION
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_BUILDING_CONSTRUCTION_id, 
            {AppConfiguration.TargetDataModelCode}_WALL_CONSTRUCTION_id,
            Wall_perc, Wall_description, Wall_material)
            select	hbc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                hbc.{AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION_id, 
			    ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) as GH_WALL_CONSTRUCTION_id, NULL as Wall_perc,
			    hbc.Details_Walls as Wall_Description, 
			    (select UDL_IUA_WALL_MATERIAL_id from UDL_IUA_WALL_MATERIAL join pmcaption 
                    on pmcaption.caption_id = UDL_IUA_WALL_MATERIAL.caption_id and caption ='Other') as Wall_material
            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS hgd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hgd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION hbc 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hbc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
            where  NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_WALL_CONSTRUCTION 
                                    where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
					                and hbc.{AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION_id = {AppConfiguration.TargetDataModelCode}_BUILDING_CONSTRUCTION_id)";


        /// <summary>
        /// Migrate new data models which have data in the source Building Construction table to Roof construction table in source table for Guest House
        /// </summary>
        public string Roof_Construction { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_ROOF_CONSTRUCTION
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_BUILDING_CONSTRUCTION_id, 
            {AppConfiguration.TargetDataModelCode}_ROOF_CONSTRUCTION_id, 
            Roof_description, Roof_perc, Roof_material)
            select	hbc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                hbc.{AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION_id, 
			    ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) as GH_ROOF_CONSTRUCTION_id,
			    hbc.Roof_details as Roof_Description, NULL as Roof_perc,  
			    (select UDL_IUA_Roof_Material_id from UDL_IUA_Roof_Material join pmcaption 
                    on pmcaption.caption_id = UDL_IUA_Roof_Material.caption_id and caption ='Other') 
                as Roof_material
            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS hgd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hgd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION hbc 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hbc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
            where  NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_ROOF_CONSTRUCTION 
                                where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
					                and hbc.{AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION_id = {AppConfiguration.TargetDataModelCode}_BUILDING_CONSTRUCTION_id)";

        /// <summary>
        /// Migrate new data models which have data in the source Building Construction table to Risk details table in source table for Guest House
        /// </summary>
        public string Risk_Details { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_RISK_DETAILS
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_BUILDING_CONSTRUCTION_id, 
            {AppConfiguration.TargetDataModelCode}_RISK_DETAILS_id, n)
            select hbc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                hbc.{AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION_id, 
                hrd.{AppConfiguration.SourceDataModelCode}_RISK_DETAILS_id, 
                NULL
            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS hgd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hgd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION hbc 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hbc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_RISK_DETAILS hrd 
                    on  hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hrd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
            where  NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_RISK_DETAILS 
                                    where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
					                and hbc.{AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION_id = {AppConfiguration.TargetDataModelCode}_BUILDING_CONSTRUCTION_id)";

        /// <summary>
        /// Migrate new data models which have data in the source Building Construction table to Risk information table in source table for Guest House
        /// </summary>
        public string Risk_Information { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_RISK_INFORMATION
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_RISK_INFORMATION_id, Flooded, Details_flood,
            Been_flooded, Details_been_flooded, Survey, Details_survey,
            Subsidence, Details_subsidence, Subsidence_cover_required)
            select	hbc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                hrd.{AppConfiguration.SourceDataModelCode}_RISK_DETAILS_id, hbc.Flooded, hbc.Flood_Details, 
                hbc.Been_flooded, hbc.Underground_drains, hbc.Survey, hbc.Surevy_details, 
                hbc.Subsidence, hbc.Subsidence_details, NULL as Subsidence_cover_required
            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS hgd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hgd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION hbc 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hbc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_RISK_DETAILS hrd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hrd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
            where  NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_RISK_INFORMATION 
                                    where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
					                    and hrd.{AppConfiguration.SourceDataModelCode}_RISK_DETAILS_id = {AppConfiguration.TargetDataModelCode}_RISK_INFORMATION_id)";

        /// <summary>
        /// Migrate new data models which have data in the source Subsidence Assessment table for Guest House
        /// </summary>
        public string Subsidence_Assessment { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_SUBSIDENCE_ASSESSMENT
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_SUBSIDENCE_ASSESSMENT_id, In_these_buildings,
            Adjoining_property, Details_subsidence, Foundations_details, Watercourses,
            Watercourses_details, Cliffs, Details_cliffs, Any_trees_nearby,
            Subsidence_claim, Details_sub_claims, State_of_site, Building_foundations)
            select	hsa.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                hsa.{AppConfiguration.SourceDataModelCode}_SUBSIDENCE_ASSESSMENT_id, hsa.In_these_buildings,
			    hsa.Adjoining_property, hsa.Details_sub, hsa.Foundations_Details, hsa.Watercourses,
			    hsa.Watercourses_details, hsa.Cliffs, hsa.Details_c, hsa.Any_Trees_nearby,
			    hsa.Claim, hsa.Claim_Details,
			    case when exists (select UDL_GH_State_of_site_id from UDL_GH_State_of_site 
									where UDL_GH_State_of_site_id=hsa.State_of_site)
				    then 
					    (select c.UDL_IUA_SITE_DES_id 
						    from Generic_Lookup_Data a 
                                join UDL_GH_State_of_site b on a.Source_Code = b.code
							    join UDL_IUA_SITE_DES c on a.Target_Code = c.code
					        where LookUp_Name='LUSUBSIDENCESiteDescription' 
                                    and UDL_GH_State_of_site_id=hsa.State_of_site)
				    else
					    NULL
			    end as State_of_site,
			    case when exists (select UDL_GH_Foundations_id from UDL_GH_Foundations 
							        where UDL_GH_Foundations_id=hsa.Foundations)
				    then 
					    (select c.UDL_IUA_BUILDING_FOUND_id 
						    from Generic_Lookup_Data a 
                                join UDL_GH_Foundations b on a.Source_Code = b.code
							    join UDL_IUA_BUILDING_FOUND c on a.Target_Code = c.code
					        where LookUp_Name='LUSUBSIDENCEBuildingFoundations' 
                                    and UDL_GH_Foundations_id=hsa.Foundations)
				    else
					    NULL
			    end as Foundations
            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS hgd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hgd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id 
                join {AppConfiguration.SourceDataModelCode}_SUBSIDENCE_ASSESSMENT hsa 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hsa.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
            where NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_SUBSIDENCE_ASSESSMENT 
                                where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
					                and hsa.{AppConfiguration.SourceDataModelCode}_SUBSIDENCE_ASSESSMENT_id = {AppConfiguration.TargetDataModelCode}_SUBSIDENCE_ASSESSMENT_id)";

        /// <summary>
        /// Migrate new data models which have data in the source Trees table for Guest House
        /// </summary>
        public string Trees_Details { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_TREES_DETAILS
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_SUBSIDENCE_ASSESSMENT_id, 
            {AppConfiguration.TargetDataModelCode}_TREES_DETAILS_id,
             Height, Distance, Type_of_tree, Details_trees, Maintenance)
            select	ht.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                ht.{AppConfiguration.SourceDataModelCode}_SUBSIDENCE_ASSESSMENT_id, 
                ht.{AppConfiguration.SourceDataModelCode}_TREES_id,
			    ht.Height, ht.Distance, ht.Type, ht.Details, ht.Maintenance
            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS hgd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hgd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id 
                join {AppConfiguration.SourceDataModelCode}_SUBSIDENCE_ASSESSMENT hsa 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hsa.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_TREES ht 
                    on hsa.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = ht.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
						and hsa.{AppConfiguration.SourceDataModelCode}_SUBSIDENCE_ASSESSMENT_id = ht.{AppConfiguration.SourceDataModelCode}_SUBSIDENCE_ASSESSMENT_id
            where  NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_TREES_DETAILS 
                                    where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
					                    and hsa.{AppConfiguration.SourceDataModelCode}_SUBSIDENCE_ASSESSMENT_id = {AppConfiguration.TargetDataModelCode}_SUBSIDENCE_ASSESSMENT_id
					                    and ht.{AppConfiguration.SourceDataModelCode}_TREES_id = {AppConfiguration.TargetDataModelCode}_TREES_DETAILS_id)";

        /// <summary>
        /// Migrate new data models which have data in the source Sums_Insured and Sums_Insured_HH table for Guest House
        /// </summary>
        public string Premises_Eqipment { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_PREMISES_EQUIPMENT
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_PREMISES_EQUIPMENT_id, Seasonal_basis,
            Details_seasonal, Reside_on_the_premises, Work_away, Details_work_away,
            Unoccupied, Details_unoccupied, Childrens_play_equipment, Details_play_equipment,
            Bouncy_castle, Details_bouncy_castle, Leisure_facilities, Indoor_swimming_pool,
            Outdoor_swimming_pool, Sauna, Jacuzzi, Gym, Treatment_room,
            Any_other_leisure_facilities, Details_leisure_facilities, Open_to_non_residents,
            Fireworks, HS_Building_Use, HS_Details_building_use, HH_Being_let,
            HH_Staff_accomodation, HH_Working_farm, HH_Bar, HH_Restaurant)
            select	hbc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) as GH_PREMISES_EQUIPMENT_Id, 
			    hbc.Seasonal_basis, hbc.Seasonal_details, NULL as Reside_on_the_premises, hbc.Work_away,
			    hbc.Work_away_details, hbc.Unoccupied, hbc.unocc_details, hbc.Childrens_Play_Equipment,
			    hbc.Play_Equip_Details, hbc.Bouncy_castle, hbc.Bouncy_Castles_Details, hbc.Leisure_Facilities,
			    hbc.Indoor_SP, hbc.Outdoor_sp, hbc.Sauna, hbc.Jaxuzzi,
			    NULL as Gym, NULL as Treatment_room, hbc.Any_other, hbc.Any_other_details,
			    hbc.Non_residents, hbc.Fireworks, NULL as HS_Building_Use, NULL as HS_Details_building_use,
			    NULL as HH_Being_let, NULL as HH_Staff_accomodation, hsihh.working_farm as HH_Working_farm, 
			    hbc.Bar as HH_Bar, hbc.Restaurant as HH_Restaurant
            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS hgd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hgd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION hbc 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hbc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_SUMS_INSURED hsi 
                    on hsi.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_SUMS_INSURED_HH hsihh 
                    on hsihh.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
            where NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_PREMISES_EQUIPMENT 
                                where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id)";

        /// <summary>
        /// Migrate new data models which have data in the source Guest House Details table for Guest House
        /// </summary>
        public string Guest_House_Details { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_GUEST_HOUSE_DETAILS
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_GUEST_HOUSE_DETAILS_id, 
            Use_of_building, Details_use_of_buildings, Owner_lives_on, Marquee, 
            Details_marquee, Letting_bedrooms, Self_catering_basis, Details_self_catering,
            Food_paying_guest, Food, Alcohol, Other, Details_facilities,
            Stag_hen, Live_entertainment, On_a_farm, Self_catering_perc)
            select	hbc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) as GH_GUEST_HOUSE_DETAILS_id, 
			    hbc.Use_of_building, hbc.Use_Details, hbc.Owner_lives_on, hbc.Marquee,
			    hbc.Marque_details, hbc.Letting_bedrooms, hbc.Self_catering_basis, hbc.Self_catering_details,
			    hbc.Food_paying_guest, NULL as Food, NULL as Alcohol, NULL as Other, NULL as Details_facilities,
			    hbc.Stag_hen, NULL as Live_entertainment, hbc.On_A_Farm, 
                case when ISNUMERIC(hbc.Self_catering_details) = 1
				    then 
					    hbc.Self_catering_details
				    else
					    NULL
			    end as Self_catering_perc
            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS hgd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hgd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_BUILDING_CONSTRUCTION hbc 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hbc.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
            where  NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_GUEST_HOUSE_DETAILS 
                                    where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id)";

        /// <summary>
        /// Migrate new data models which have data in the source Guest House sum insured table for Guest House
        /// </summary>
        public string Guest_House_Sum_Insured { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_GUEST_HOUSE_SUM_INSURED
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_GUEST_HOUSE_SUM_INSURED_id, GH_Buildings_SI,
            GH_Inner_limits, GH_total_wine, GH_total_computer, GH_portable_possessions,
            PP_unspecified_items, PP_Total_specified_items, PP_Details_safe_make_model, GH_Frozen_food_limit,
            GH_Frozen_food_SI, GH_Solar_panels, GH_Solar_panels_value, GH_biomass_boilers, GH_biomass_boilers_value,
            GH_contents_SI, GH_Value_solar_panels, GH_biomass_boilers_stored)
            select	hsi.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                hsi.{AppConfiguration.SourceDataModelCode}_SUMS_INSURED_id, 
			    hsi.Buildings as GH_Buildings_SI, hsi.Contents as GH_Inner_limits, hsi.Total_wine as GH_Total_wine, 
			    hsi.Total_computer as GH_Total_computer, hsi.Portable_possesions as GH_Portable_possessions, 
			    hp.Unspecified_items as PP_Unspecified_items, hp.Total_Specified_Items as PP_Total_specified_items, 
			    hp.safe_details as PP_Details_safe_make_model, hsi.Frozen_food_limit as GH_Frozen_food_limit,
			    hsi.Frozen_Food as GH_Frozen_Food_SI, NULL as GH_Solar_panels, NULL as GH_Solar_Panels_Value, 
			    NULL as GH_Biomass_boilers, NULL as GH_biomass_boilers_value, 
			    case when exists (select UDL_GH_Contents_id from UDL_GH_Contents where UDL_GH_Contents_id=hsi.Contents_amount)
				    then 
				         (select c.UDL_IUA_CONTENTS_SI_id from Generic_Lookup_Data a 
                                join UDL_GH_Contents b on a.Source_Code = b.code
				                join UDL_IUA_CONTENTS_SI c on a.Target_Code = c.code
				            where LookUp_Name='LUContentsSI' and UDL_GH_Contents_id=hsi.Contents_amount)
				    else
				        NULL
				end as GH_Contents_SI, 
			    NULL as GH_Value_solar_panels, NULL as GH_biomass_boilers_stored
            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS hgd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hgd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_SUMS_INSURED hsi 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hsi.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_POSSESSIONS hp 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hp.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
            where NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_GUEST_HOUSE_SUM_INSURED 
						            where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
							            and hsi.{AppConfiguration.SourceDataModelCode}_SUMS_INSURED_id = {AppConfiguration.TargetDataModelCode}_GUEST_HOUSE_SUM_INSURED_id)";

        /// <summary>
        /// Migrate new data models which have data in the source Guest House sum insured 2 table for Guest House
        /// </summary>
        public string Guest_House_Sum_Insured_2 { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_GUEST_HOUSE_SUM_INSURED_2
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_GUEST_HOUSE_SUM_INSURED_2_id, GH_business_interuption_SI,
            GH_loss_of_licence_SI, GH_book_debts_SI, GH_goods_in_transit_SI, GH_guest_effect_SI,
            GH_property_in_the_open_SI, GH_lock_replacement_SI, GH_loss_of_oil_SI, GH_fixed_signs_SI,
            GH_employers_liability_SI, GH_cash_on_premises_SI, GH_Cash_in_safe_SI, GH_cash_limits_sufficient,
            GH_cash_cover_required, GH_Details_safe_model, GH_increase_excess, GH_employee_effects_SI,
            GH_public_liability_SI, GH_excess_changes)
            select	hsi.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                hsi.{AppConfiguration.SourceDataModelCode}_SUMS_INSURED_id, 
			        NULL as GH_business_interuption_SI,NULL as GH_loss_of_licence_SI,NULL as GH_book_debts_SI,
                    NULL as GH_goods_in_transit_SI,NULL as GH_guest_effect_SI,NULL as GH_property_in_the_open_SI,
                    NULL as GH_lock_replacement_SI,NULL as GH_loss_of_oil_SI,NULL as GH_fixed_signs_SI,
			        NULL as GH_employers_liability_SI,NULL as GH_cash_on_premises_SI,NULL as GH_Cash_in_safe_SI,
			        hsi.cash_limits_sufficent as GH_cash_limits_sufficent,
			        ISNULL(hsi.INCCIS, 0) + ISNULL(hsi.INCCIT, 0) as GH_cash_cover_required, 
			        hsi.Make_Model_safe as GH_Details_safe_model,
			        NULL as GH_increase_excess,NULL as GH_employee_effects_SI,
			        case when exists (select UDL_GH_Public_Liability_id from UDL_GH_Public_Liability 
						                where UDL_GH_Public_Liability_id=hsi.Public_liability)
				        then 
				         (select c.UDL_IUA_PUBLIC_LIABILITY_id 
					        from Generic_Lookup_Data a join UDL_GH_Public_Liability b on a.Source_Code = b.code
						        join UDL_IUA_PUBLIC_LIABILITY c on a.Target_Code = c.code
					        where LookUp_Name='LUPublicLiability' and UDL_GH_Public_Liability_id=hsi.Public_liability)
				        else
				            NULL
				    end as GH_public_liability_SI,
			        NULL as GH_excess_changes
            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS hgd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hgd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_SUMS_INSURED hsi 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hsi.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
            where  NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_GUEST_HOUSE_SUM_INSURED_2 
						            where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
							            and hsi.{AppConfiguration.SourceDataModelCode}_SUMS_INSURED_id = {AppConfiguration.TargetDataModelCode}_GUEST_HOUSE_SUM_INSURED_2_id)";

        /// <summary>
        /// Migrate new data models which have data in the source Specified items and possession table for Guest House
        /// </summary>
        public string Portable_Possessions { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_PORTABLE_POSSESSIONS
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_GUEST_HOUSE_SUM_INSURED_id, 
            {AppConfiguration.TargetDataModelCode}_PORTABLE_POSSESSIONS_id,
            PP_Details_specified_values, PP_Specified_value, PP_Valuation_seen, PP_Specified_items)
            select	si.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                    hsi.{AppConfiguration.SourceDataModelCode}_SUMS_INSURED_id,
			        si.{AppConfiguration.SourceDataModelCode}_SPECIFIED_ITEMS_id, si.Item as PP_Details_specified_values, 
			        si.Amount as PP_Specified_value, si.Valuation as PP_Valuation_seen,
			        case when exists (select UDL_GH_SPEC_ITEMS_id from UDL_GH_SPEC_ITEMS 
								            where UDL_GH_SPEC_ITEMS_id=si.Specified_items)
				        then 
					        (select c.UDL_IUA_PP_SPEC_ITEMS_id 
						        from Generic_Lookup_Data a join UDL_GH_SPEC_ITEMS b on a.Source_Code = b.code
							        join UDL_IUA_PP_SPEC_ITEMS c on a.Target_Code = c.code
						        where LookUp_Name='LUSpecifieditems' and UDL_GH_SPEC_ITEMS_id=si.Specified_items)
				        else
				            NULL
			        end as PP_Specified_items
            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS hgd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hgd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_SUMS_INSURED hsi 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hsi.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_SPECIFIED_ITEMS si 
                    on si.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = hsi.{AppConfiguration.SourceDataModelCode}_Policy_binder_id 
                join {AppConfiguration.SourceDataModelCode}_POSSESSIONS hp 
                    on si.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hp.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
					   and si.{AppConfiguration.SourceDataModelCode}_POSSESSIONS_id = hp.{AppConfiguration.SourceDataModelCode}_POSSESSIONS_id
            where NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_PORTABLE_POSSESSIONS 
						            where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
							            and hsi.{AppConfiguration.SourceDataModelCode}_SUMS_INSURED_id = {AppConfiguration.TargetDataModelCode}_GUEST_HOUSE_SUM_INSURED_id
							            and si.{AppConfiguration.SourceDataModelCode}_SPECIFIED_ITEMS_id = {AppConfiguration.TargetDataModelCode}_PORTABLE_POSSESSIONS_id)";

        /// <summary>
        /// Migrate new data models which have data in the source Holiday Home sum insured table for Guest House
        /// </summary>
        public string Holiday_Home_Sum_Insured { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_HOLIDAY_HOME_SUM_INSURED
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_HOLIDAY_HOME_SUM_INSURED_id, HH_Buildings_SI, HH_Contents_SI,
            HH_Inner_contents_limits, HH_Stock_wines, HH_Computers_electronics, HH_Contents_AD, 
            HH_Business_interruption_SI, HH_Book_debts_SI, HH_Guest_effects_SI, HH_Business_money_SI,
            HH_Employee_effects_SI, HH_Property_in_the_open_SI, HH_Lock_replacement_SI, HH_Loss_of_oil_SI,
            HH_Damage_to_signs_SI, HH_Public_liability_SI, HH_Employers_liability_SI, HH_Solar_panels,
            HH_Biomass_boilers, HH_Biomass_boilers_stored, HH_Biomass_boilers_value, HH_Increase_excess,
            HH_Excess_changes, HH_Solar_panels_value, HH_Solar_panel_value)
            select	hsihh.{AppConfiguration.SourceDataModelCode}_Policy_binder_id, 
                    hsihh.{AppConfiguration.SourceDataModelCode}_SUMS_INSURED_HH_id,
			        hsihh.Buildings as HH_Buildings_SI, hsihh.Contents as HH_Contents_SI, NULL as HH_Inner_contents_limits,
			        NULL as HH_Stock_wines, NULL as HH_Computers_electronics, hsihh.Contents_AD as HH_Contents_AD,
			        NULL as HH_Business_interruption_SI,NULL as HH_Book_debts_SI,NULL as HH_Guest_effects_SI,
                    NULL as HH_Business_money_SI,NULL as HH_Employee_effects_SI,NULL as HH_Property_in_the_open_SI,
                    NULL as HH_Lock_replacement_SI,NULL as HH_Loss_of_oil_SI,NULL as HH_Damage_to_signs_SI,
			        hsihh.Public_Liability as HH_Public_liability_SI, NULL as HH_Employers_liability_SI,NULL as HH_Solar_panels,
			        NULL as HH_Biomass_boilers,NULL as HH_Biomass_boilers_stored,NULL as HH_Biomass_boilers_value,
			        NULL as HH_Increase_excess,NULL as HH_Excess_changes,NULL as HH_Solar_panels_value,
                    NULL as HH_Solar_panel_value
            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS hgd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hgd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_SUMS_INSURED_HH hsihh 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hsihh.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
            where NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_HOLIDAY_HOME_SUM_INSURED 
						            where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
							            and hsihh.{AppConfiguration.SourceDataModelCode}_SUMS_INSURED_HH_id = {AppConfiguration.TargetDataModelCode}_HOLIDAY_HOME_SUM_INSURED_id)";

        /// <summary>
        /// Migrate new data models which have data in the source Staff Account sum insured table for Guest House
        /// </summary>
        public string Staff_Acc_Sum_Insured { get; set; } = $@"----------/**/----------
            INSERT INTO dbo.{AppConfiguration.TargetDataModelCode}_STAFF_ACC_SUM_INSURED
            ({AppConfiguration.TargetDataModelCode}_Policy_binder_id, 
            {AppConfiguration.TargetDataModelCode}_STAFF_ACC_SUM_INSURED_id, SA_Buildings, SA_Landlord_contents,
            SA_Property_in_the_open, SA_Lock_replacement, SA_Loss_of_oil, SA_Public_liability, SA_Solar_panels,
            SA_Biomass_boilers, SA_Biomass_boilers_stored, SA_Biomass_boilers_value, SA_Principal_Bus,
            SA_Occuping_risk_address, SA_Excess_charge, SA_Excess_discount, SA_Solar_panels_value,
            SA_Employers_liability)
            select	hsisa.{AppConfiguration.SourceDataModelCode}_Policy_binder_id,
                    hsisa.{AppConfiguration.SourceDataModelCode}_SUMS_INSURED_SA_id,
			        hsisa.Buildings as SA_Buildings, hsisa.Landlord_Contents as SA_Landlord_contents, NULL as SA_Property_in_the_open,
			        NULL as SA_Lock_replacement,NULL as SA_Loss_of_oil,hsisa.Public_Liability as SA_Public_liability,
			        NULL as SA_Solar_panels,NULL as SA_Biomass_boilers,NULL as SA_Biomass_boilers_stored,
                    NULL as SA_Biomass_boilers_value,hsisa.Principal_bus as SA_Principal_Bus,
			        hsisa.occuping_risk_address as SA_Occuping_risk_address,
                    NULL as SA_Excess_charge,NULL as SA_Excess_discount,NULL as SA_Solar_panels_value,
                    NULL as SA_Employers_liability
            from {AppConfiguration.SourceDataModelCode}_Policy_Binder hpb 
                join {AppConfiguration.SourceDataModelCode}_GENERAL_DETAILS hgd 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hgd.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
                join {AppConfiguration.SourceDataModelCode}_SUMS_INSURED_SA hsisa 
                    on hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id=hsisa.{AppConfiguration.SourceDataModelCode}_Policy_binder_id
            where  NOT exists (select 1 from {AppConfiguration.TargetDataModelCode}_STAFF_ACC_SUM_INSURED 
						            where hpb.{AppConfiguration.SourceDataModelCode}_Policy_binder_id = {AppConfiguration.TargetDataModelCode}_Policy_binder_id
							            and hsisa.{AppConfiguration.SourceDataModelCode}_SUMS_INSURED_SA_id = {AppConfiguration.TargetDataModelCode}_STAFF_ACC_SUM_INSURED_id)";

        #endregion


        public void ExecuteOutputTables()
        {
         string SourceOutputTables = "SELECT table_name FROM information_schema.tables WHERE table_name like'" + AppConfiguration.SourceDataModelCode + "_Output%'";
        IGenericRepository<OutputTable> repoContext = new GenericRepository<OutputTable>();
            //repoContext.ExecuteSqlCommand(Policy_binder);
            //repoContext.ExecuteSqlCommand(DataModelCode_Policy);
            List<OutputTable> result = repoContext.Query(SourceOutputTables).ToList();
          
            foreach (OutputTable tr in result)
            {
                
                string getOutputTables= "if exists(select table_name FROM information_schema.tables WHERE table_name ='"+tr.table_name.Replace(AppConfiguration.SourceDataModelCode, AppConfiguration.TargetDataModelCode) + "')select 1 else select 0";
                bool checkOutputTableExist = Convert.ToBoolean(repoContext.CheckTableExist(getOutputTables));
                if(checkOutputTableExist)
                {
                    repoContext.ExecuteSqlCommand(" insert into " + tr.table_name.Replace(AppConfiguration.SourceDataModelCode, AppConfiguration.TargetDataModelCode)
                        + " select * from " + tr.table_name);                  
                }

                //Helper.WriteToFile($"--------------------{scriptName}--------------------", AppConfiguration.File_SqlFileName);
                //Helper.WriteToFile(script, AppConfiguration.File_SqlFileName);
                //Helper.WriteToFile($"", AppConfiguration.File_SqlFileName);
                //Helper.WriteToFile($"GO", AppConfiguration.File_SqlFileName);
                //Helper.WriteToFile($"", AppConfiguration.File_SqlFileName);

                //Helper.WriteToFile($"{Helper.GetCurrenctDate()} -- {scriptName} - Execution Time {Helper.GetTimeDifference(startTime, endTime)}", AppConfiguration.File_FileName);
                else
                {
                    Helper.WriteToFile("Table"+ tr.table_name + " Not Found in Target DataModelCode" , AppConfiguration.File_FileName);
                   
                }
            }
        }

    }

}
