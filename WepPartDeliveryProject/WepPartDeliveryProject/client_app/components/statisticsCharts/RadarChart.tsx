import { ChartData } from "chart.js";
import { FC, useEffect, useState } from "react";
import { Radar } from 'react-chartjs-2';
import ChartDataLabels from 'chartjs-plugin-datalabels';

import {
    Chart as ChartJS,
    RadialLinearScale,
    PointElement,
    LineElement,
    Filler,
    Tooltip,
    Legend,
  } from 'chart.js';
  
  ChartJS.register(
    RadialLinearScale,
    PointElement,
    LineElement,
    Filler,
    Tooltip,
    Legend
  );

function getRandomInt(max:number) {
    return Math.floor(Math.random() * max);
}

const RadarChart: FC<{ query:statisticQueryInfo }> = ({ query }) => {
    const [chartData, setChartData] = useState<ChartData<"radar">>();

    useEffect(() => {
        const fetchData = async () => {
            const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/statistic/${query.linkToQuery}`, {
                headers: {
                    'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                },
            });
            const resInfo = await resp.json() as {queryData: statisticQueryDataItem[], nameDatasets:string[]};
    
            const data:{labels:string[], datasets:any} = {
                labels: resInfo.queryData.map((value) => value.x),
                datasets: [],
            }
            
            for( let i = 0; i < resInfo.nameDatasets.length; i++)
            {
                data.datasets.push({
                    label: resInfo.nameDatasets[i] ,
                    data: resInfo.queryData.map(value => value.y[i]),
                    borderColor: `rgb(${getRandomInt(255)}, ${getRandomInt(255)}, ${getRandomInt(255)}, 0.5)`,
                    backgroundColor: `rgb(${getRandomInt(255)}, ${getRandomInt(255)}, ${getRandomInt(255)}, 0.3)`,
                    borderWidth: 1,
                    fill: true,
                });
            };

            setChartData(data);
        }
        fetchData();
    }, [query]);

    const options = {
        plugins: {
            datalabels: {
                color: '#000',
                font: {
                    weight: 'bold' as 'bold'
                },
                align: 'top' as 'top'
              }
        },
    };

    return (
        <>
            {chartData != undefined && <Radar plugins={[ChartDataLabels]} options={options} data={chartData}/> }
        </>    
    );
}

export default RadarChart;