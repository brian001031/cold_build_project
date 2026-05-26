import React, { Suspense } from 'react';
import TimelyReport from './timelyReport';
import PastReport from './pastReport';


const DayReport = () => {

    

    return(
        <div className=''>
            <Suspense fallback={<div>Loading...</div>}  >
                <TimelyReport />
                <PastReport />
            </Suspense>
        </div>
    )
    
}
export default DayReport;